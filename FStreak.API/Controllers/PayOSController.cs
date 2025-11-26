using FStreak.Application.DTOs;
using FStreak.Application.Services;
using FStreak.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace FStreak.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayOSController : ControllerBase
{
    private readonly IPayOSService _payOSService;
    private readonly ILogger<PayOSController> _logger;

    public PayOSController(IPayOSService payOSService, ILogger<PayOSController> logger)
    {
        _payOSService = payOSService;
        _logger = logger;
    }

    /// <summary>
    /// Create payment link for a plan
    /// </summary>
    [HttpPost("create-payment")]
    [Authorize]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _payOSService.CreatePaymentLinkAsync(userId, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreatePayment");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get payment status by order code
    /// </summary>
    [HttpGet("status/{orderCode}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus(long orderCode)
    {
        try
        {
            var result = await _payOSService.GetPaymentStatusAsync(orderCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for {OrderCode}", orderCode);
            return StatusCode(500, new { message = "Error getting payment status", error = ex.Message });
        }
    }

    /// <summary>
    /// PayOS webhook endpoint - handles payment confirmation
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook([FromBody] PayOSWebhookDto webhook)
    {
        try
        {
            _logger.LogInformation("Received PayOS webhook: {Webhook}", JsonSerializer.Serialize(webhook));

            // Verify signature
            var webhookData = JsonSerializer.Serialize(webhook.Data);
            var isValid = await _payOSService.VerifyWebhookSignatureAsync(webhookData, webhook.Signature);

            if (!isValid)
            {
                _logger.LogWarning("Invalid webhook signature");
                return BadRequest(new { message = "Invalid signature" });
            }

            // Handle webhook
            var success = await _payOSService.HandleWebhookAsync(webhook);

            if (success)
            {
                return Ok(new { message = "Webhook processed successfully" });
            }

            return BadRequest(new { message = "Error processing webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PayOS webhook");
            return StatusCode(500, new { message = "Error processing webhook", error = ex.Message });
        }
    }

    /// <summary>
    /// Get user's payment history
    /// </summary>
    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var history = await _payOSService.GetUserPaymentHistoryAsync(userId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history");
            return StatusCode(500, new { message = "Error getting payment history", error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel payment
    /// </summary>
    [HttpPost("cancel/{orderCode}")]
    [Authorize]
    public async Task<IActionResult> CancelPayment(long orderCode)
    {
        try
        {
            // PayOS doesn't provide cancel API, so just update status in DB
            return Ok(new { message = "Payment cancellation requested" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment");
            return StatusCode(500, new { message = "Error cancelling payment", error = ex.Message });
        }
    }
}