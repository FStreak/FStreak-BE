using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FStreak.Application.Services.Implementation
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayOSService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepo;
        private readonly string _checksumKey;

        public PayOSService(
            IConfiguration configuration,
            ILogger<PayOSService> logger,
            IUnitOfWork unitOfWork, 
            IUserRepository userRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userRepo = userRepository;

            var clientId = _configuration["PAYOS_CLIENT_ID"];
            var apiKey = _configuration["PAYOS_API_KEY"];
            _checksumKey = _configuration["PAYOS_CHECK_SUM_KEY"] ?? "";

            // ✅ Log để debug
            _logger.LogInformation("PayOS Config - ClientId: {ClientId}, ApiKey: {ApiKey}, ChecksumKey: {ChecksumKey}", 
                string.IsNullOrEmpty(clientId) ? "MISSING" : "OK",
                string.IsNullOrEmpty(apiKey) ? "MISSING" : "OK",
                string.IsNullOrEmpty(_checksumKey) ? "MISSING" : "OK");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(_checksumKey))
            {
                throw new InvalidOperationException("PayOS credentials not configured properly");
            }

            try
            {
                var options = new PayOSOptions
                {
                    ClientId = clientId,
                    ApiKey = apiKey,
                    ChecksumKey = _checksumKey
                };
                _payOS = new PayOSClient(options);
                _logger.LogInformation("✅ PayOSClient initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize PayOSClient");
                throw;
            }
        }

        public async Task<PaymentResponseDto> CreatePaymentLinkAsync(string userId, CreatePaymentDto dto)
        {
            try
            {
                _logger.LogInformation("Creating payment for user {UserId}, amount {Amount}, planId {PlanId}", 
                    userId, dto.Amount, dto.PlanId);

                var orderCode = DateTimeOffset.Now.ToUnixTimeSeconds();
                _logger.LogInformation("Generated orderCode: {OrderCode}", orderCode);

                // ✅ Rút ngắn description NGAY TẠI ĐÂY
                var shortDescription = string.IsNullOrEmpty(dto.Description) 
                    ? "FStreak Premium" 
                    : (dto.Description.Length > 25 ? dto.Description.Substring(0, 25) : dto.Description);

                _logger.LogInformation("Original description: {Original}, Short description: {Short}", 
                    dto.Description, shortDescription);

                // Save to database first
                var payment = new Payment
                {
                    UserId = userId,
                    OrderCode = orderCode,
                    Amount = dto.Amount,
                    PlanId = dto.PlanId,
                    Status = "Pending",
                    Description = dto.Description, // Lưu description đầy đủ vào DB
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("✅ Payment saved to database");

                var returnUrl = _configuration["PAYOS_RETURN_URL"] ?? "https://fstreak.vercel.app/payment/success";
                var cancelUrl = _configuration["PAYOS_CANCEL_URL"] ?? "https://fstreak.vercel.app/payment/cancel";

                _logger.LogInformation("URLs - Return: {ReturnUrl}, Cancel: {CancelUrl}", returnUrl, cancelUrl);

                // ✅ Sử dụng description ngắn cho PayOS
                var request = new CreatePaymentLinkRequest
                {
                    OrderCode = (int)orderCode,
                    Amount = (int)dto.Amount,
                    Description = shortDescription, // ✅ Description đã rút ngắn
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                };

                var requestJson = JsonSerializer.Serialize(request);
                _logger.LogInformation("Request data: {RequestData}", requestJson);

                // ✅ Gọi PayOS API
                var response = await _payOS.PaymentRequests.CreateAsync(request);

                if (response == null)
                {
                    _logger.LogError("❌ PayOS returned null response");
                    throw new Exception("PayOS không trả về kết quả");
                }

                var responseJson = JsonSerializer.Serialize(response);
                _logger.LogInformation("✅ PayOS response: {Response}", responseJson);

                var checkoutUrl = response.CheckoutUrl ?? "";
                
                if (string.IsNullOrEmpty(checkoutUrl))
                {
                    _logger.LogError("❌ PayOS returned empty checkout URL");
                    throw new Exception("PayOS không trả về link thanh toán");
                }

                payment.PaymentUrl = checkoutUrl;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("✅ Payment link created: {Url}", checkoutUrl);

                return new PaymentResponseDto
                {
                    OrderCode = orderCode.ToString(),
                    PaymentUrl = checkoutUrl,
                    Status = "Pending"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating payment for user {UserId}", userId);
                throw new Exception($"Lỗi tạo thanh toán: {ex.Message}", ex);
            }
        }

        public async Task<PaymentResponseDto> GetPaymentStatusAsync(long orderCode)
        {
            try
            {
                var allPayments = await _unitOfWork.Payments.GetAllAsync();
                var payment = allPayments.FirstOrDefault(p => p.OrderCode == orderCode);

                if (payment == null)
                {
                    throw new InvalidOperationException($"Payment not found for orderCode {orderCode}");
                }

                try
                {
                    var paymentInfo = await _payOS.PaymentRequests.GetAsync((int)orderCode);
                    
                    if (paymentInfo != null)
                    {
                        payment.Status = paymentInfo.Status.ToString();
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get PayOS payment info for {OrderCode}", orderCode);
                }

                return new PaymentResponseDto
                {
                    OrderCode = orderCode.ToString(),
                    Status = payment.Status,
                    PaymentUrl = payment.PaymentUrl ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for orderCode {OrderCode}", orderCode);
                throw;
            }
        }

        public async Task<bool> VerifyWebhookSignatureAsync(string webhookData, string signature)
        {
            try
            {
                var computedSignature = ComputeHmacSha256(webhookData, _checksumKey);
                var isValid = computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
                _logger.LogInformation("Webhook signature verification: {Result}", isValid ? "✅ Valid" : "❌ Invalid");
                return await Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying webhook signature");
                return false;
            }
        }

        public async Task<bool> HandleWebhookAsync(PayOSWebhookDto webhook)
        {
            try
            {
                if (webhook.Data == null) return false;

                _logger.LogInformation("Processing webhook for orderCode: {OrderCode}", webhook.Data.OrderCode);

                var allPayments = await _unitOfWork.Payments.GetAllAsync();
                var payment = allPayments.FirstOrDefault(p => p.OrderCode == webhook.Data.OrderCode);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for orderCode {OrderCode}", webhook.Data.OrderCode);
                    return false;
                }

                if (webhook.Code == "00" && webhook.Success)
                {
                    payment.Status = "PAID";
                    payment.CompleteAt = DateTime.UtcNow;
                    payment.TransactionReference = webhook.Data.Reference;

                    var user = await _userRepo.GetByIdAsync(payment.UserId);
                    if (user != null)
                    {
                        await _unitOfWork.Users.UpdateAsync(user);
                        _logger.LogInformation("✅ Updated user {UserId} to premium", user.Id);
                    }

                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("✅ Payment {OrderCode} completed successfully", payment.OrderCode);
                }
                else
                {
                    payment.Status = "CANCELLED";
                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogWarning("⚠️ Payment {OrderCode} was cancelled", payment.OrderCode);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling webhook");
                return false;
            }
        }
        public async Task<List<PaymentHistoryDto>> GetAllPaymentsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all payments");

                // Use DbContext directly to include User navigation property
                var allPayments = await _unitOfWork.Payments
                    .GetQueryable()
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Total payments found: {Count}", allPayments.Count);

                var result = allPayments
                    .Select(p => new PaymentHistoryDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        UserName = p.User?.UserName ?? p.User?.Email ?? "Unknown",
                        OrderCode = p.OrderCode,
                        Amount = p.Amount,
                        PlanId = p.PlanId,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        CompleteAt = p.CompleteAt == default ? null : (DateTime?)p.CompleteAt,
                        TransactionReference = p.TransactionReference
                    })
                    .ToList();

                _logger.LogInformation("Returning {Count} payment records", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payments");
                throw;
            }
        }

        public async Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting payment history for user: {UserId}", userId);

                var userPayments = await _unitOfWork.Payments
                    .GetQueryable()
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} payments for user {UserId}", userPayments.Count, userId);

                return userPayments
                    .Select(p => new PaymentHistoryDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        UserName = p.User?.UserName ?? p.User?.Email ?? "Unknown",
                        OrderCode = p.OrderCode,
                        Amount = p.Amount,
                        PlanId = p.PlanId,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        CompleteAt = p.CompleteAt == default ? null : (DateTime?)p.CompleteAt,
                        TransactionReference = p.TransactionReference
                    }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
                throw;
            }
        }

        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}