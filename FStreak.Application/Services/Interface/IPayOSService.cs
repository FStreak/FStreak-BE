using FStreak.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services.Interface
{
    public interface IPayOSService
    {
        Task<PaymentResponseDto> CreatePaymentLinkAsync(string userId, CreatePaymentDto dto);
        Task<PaymentResponseDto> GetPaymentStatusAsync(long orderCode);
        Task<bool> VerifyWebhookSignatureAsync(string webhookData, string signature);
        Task<bool> HandleWebhookAsync(PayOSWebhookDto webhook);
        Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(string userId);
    }
    
}
