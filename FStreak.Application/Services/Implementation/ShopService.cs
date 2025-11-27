using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace FStreak.Application.Services.Implementation
{
    public class ShopService : IShopService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FStreakDbContext _context;
        private readonly IMapper _mapper;

        public ShopService(IUnitOfWork unitOfWork, FStreakDbContext context, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ShopItemDto>>> GetShopItemsAsync(bool? activeOnly = true)
        {
            try
            {
                var query = _unitOfWork.ShopItems.GetQueryable();
                if (activeOnly == true)
                {
                    query = query.Where(si => si.IsActive);
                }

                var items = await query.OrderBy(si => si.Name).ToListAsync();
                var dtos = _mapper.Map<List<ShopItemDto>>(items);
                return Result<List<ShopItemDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ShopItemDto>>.Failure($"Failed to get shop items: {ex.Message}");
            }
        }

        public async Task<Result<ShopItemDto>> GetShopItemByIdAsync(Guid id)
        {
            try
            {
                var item = await _unitOfWork.ShopItems.GetQueryable()
                    .FirstOrDefaultAsync(si => si.Id == id);
                if (item == null)
                {
                    return Result<ShopItemDto>.Failure("Shop item not found");
                }

                var dto = _mapper.Map<ShopItemDto>(item);
                return Result<ShopItemDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<ShopItemDto>.Failure($"Failed to get shop item: {ex.Message}");
            }
        }

        public async Task<Result<ShopOrderDto>> CreateOrderAsync(string userId, CreateOrderDto dto)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var user = await _unitOfWork.Users.GetQueryable()
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    if (user == null)
                    {
                        return Result<ShopOrderDto>.Failure("User not found");
                    }

                    var order = new ShopOrder
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        OrderNumber = GenerateOrderNumber(),
                        Status = ShopOrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    decimal totalAmount = 0;
                    int totalPoints = 0;
                    var orderItems = new List<ShopOrderItem>();

                    foreach (var itemRequest in dto.Items)
                    {
                        var shopItem = await _unitOfWork.ShopItems.GetQueryable()
                            .FirstOrDefaultAsync(si => si.Id == itemRequest.ShopItemId);
                        if (shopItem == null || !shopItem.IsActive)
                        {
                            await transaction.RollbackAsync();
                            return Result<ShopOrderDto>.Failure($"Shop item {itemRequest.ShopItemId} not found or inactive");
                        }

                        if (shopItem.Stock < itemRequest.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return Result<ShopOrderDto>.Failure($"Insufficient stock for item {shopItem.Name}. Available: {shopItem.Stock}, Requested: {itemRequest.Quantity}");
                        }

                        var orderItem = new ShopOrderItem
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ShopItemId = shopItem.Id,
                            Quantity = itemRequest.Quantity,
                            PriceAtPurchase = dto.PayWith == "Points" ? 0 : shopItem.Price,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        orderItems.Add(orderItem);

                        if (dto.PayWith == "Points")
                        {
                            totalPoints += shopItem.PricePoints * itemRequest.Quantity;
                        }
                        else
                        {
                            totalAmount += shopItem.Price * itemRequest.Quantity;
                        }

                        // Reduce stock
                        shopItem.Stock -= itemRequest.Quantity;
                        shopItem.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.ShopItems.UpdateAsync(shopItem);
                    }

                    order.TotalAmount = totalAmount;
                    order.TotalPoints = totalPoints;

                    // TODO: Check if user has enough points/money and deduct
                    // This would require adding a Points property to ApplicationUser or a separate UserPoints entity

                    await _unitOfWork.ShopOrders.AddAsync(order);
                    foreach (var orderItem in orderItems)
                    {
                        await _unitOfWork.ShopOrderItems.AddAsync(orderItem);
                    }

                    order.Status = ShopOrderStatus.Completed;
                    await _unitOfWork.ShopOrders.UpdateAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Reload order with items
                    var reloadedOrder = await _unitOfWork.ShopOrders.GetQueryable()
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.ShopItem)
                        .FirstOrDefaultAsync(o => o.Id == order.Id);

                    var orderDto = _mapper.Map<ShopOrderDto>(reloadedOrder);
                    return Result<ShopOrderDto>.Success(orderDto);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Result<ShopOrderDto>.Failure($"Failed to create order: {ex.Message}");
            }
        }

        public async Task<Result<ShopOrderDto>> GetOrderByIdAsync(Guid id, string userId)
        {
            try
            {
                var order = await _unitOfWork.ShopOrders.GetQueryable()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ShopItem)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    return Result<ShopOrderDto>.Failure("Order not found");
                }

                var dto = _mapper.Map<ShopOrderDto>(order);
                return Result<ShopOrderDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<ShopOrderDto>.Failure($"Failed to get order: {ex.Message}");
            }
        }

        public async Task<Result<List<ShopOrderDto>>> GetUserOrdersAsync(string userId)
        {
            try
            {
                var orders = await _unitOfWork.ShopOrders.GetQueryable()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ShopItem)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var dtos = _mapper.Map<List<ShopOrderDto>>(orders);
                return Result<List<ShopOrderDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ShopOrderDto>>.Failure($"Failed to get user orders: {ex.Message}");
            }
        }

        public async Task<Result<ShopOrderDto>> BuyItemAsync(string userId, BuyItemRequest request)
        {
            var createOrderDto = new CreateOrderDto
            {
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ShopItemId = request.ShopItemId,
                        Quantity = request.Quantity
                    }
                },
                PayWith = request.PayWith
            };

            return await CreateOrderAsync(userId, createOrderDto);
        }

        public async Task<Result<ShopItemDto>> CreateShopItemAsync(CreateShopItemDto dto)
        {
            try
            {
                var exists = await _unitOfWork.ShopItems.GetQueryable()
                    .AnyAsync(si => si.Code == dto.Code);
                if (exists)
                {
                    return Result<ShopItemDto>.Failure("Shop item code already exists");
                }

                if (!Enum.TryParse<ShopItemType>(dto.Type, out var itemType))
                {
                    return Result<ShopItemDto>.Failure($"Invalid shop item type: {dto.Type}");
                }

                var shopItem = new ShopItem
                {
                    Id = Guid.NewGuid(),
                    Code = dto.Code,
                    Name = dto.Name,
                    Description = dto.Description,
                    Type = itemType,
                    Price = dto.Price,
                    PricePoints = dto.PricePoints,
                    Stock = dto.Stock,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ShopItems.AddAsync(shopItem);
                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.ShopItems.GetQueryable()
                    .FirstOrDefaultAsync(si => si.Id == shopItem.Id);
                var shopItemDto = _mapper.Map<ShopItemDto>(result!);

                return Result<ShopItemDto>.Success(shopItemDto);
            }
            catch (Exception ex)
            {
                return Result<ShopItemDto>.Failure($"Failed to create shop item: {ex.Message}");
            }
        }

        public async Task<Result<ShopItemDto>> UpdateShopItemAsync(Guid id, UpdateShopItemDto dto)
        {
            try
            {
                var shopItem = await _unitOfWork.ShopItems.GetQueryable()
                    .FirstOrDefaultAsync(si => si.Id == id);
                if (shopItem == null)
                {
                    return Result<ShopItemDto>.Failure("Shop item not found");
                }

                if (dto.Name != null) shopItem.Name = dto.Name;
                if (dto.Description != null) shopItem.Description = dto.Description;
                if (dto.Price.HasValue) shopItem.Price = dto.Price.Value;
                if (dto.PricePoints.HasValue) shopItem.PricePoints = dto.PricePoints.Value;
                if (dto.Stock.HasValue) shopItem.Stock = dto.Stock.Value;
                if (dto.IsActive.HasValue) shopItem.IsActive = dto.IsActive.Value;
                if (dto.Image != null) shopItem.Image = dto.Image;
                shopItem.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ShopItems.UpdateAsync(shopItem);
                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.ShopItems.GetQueryable()
                    .FirstOrDefaultAsync(si => si.Id == id);
                var shopItemDto = _mapper.Map<ShopItemDto>(result!);

                return Result<ShopItemDto>.Success(shopItemDto);
            }
            catch (Exception ex)
            {
                return Result<ShopItemDto>.Failure($"Failed to update shop item: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteShopItemAsync(Guid id)
        {
            try
            {
                var shopItem = await _unitOfWork.ShopItems.GetQueryable()
                    .FirstOrDefaultAsync(si => si.Id == id);
                if (shopItem == null)
                {
                    return Result<bool>.Failure("Shop item not found");
                }

                await _unitOfWork.ShopItems.DeleteAsync(shopItem);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to delete shop item: {ex.Message}");
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}

