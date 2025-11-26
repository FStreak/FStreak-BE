using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.DTOs
{
    public class CreatePaymentDto
    {
        public string PlanId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        //public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }

    public class PaymentResponseDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PayOSWebhookDto
    {
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public bool Success { get; set; }
        public PayOSWebhookData? Data { get; set; }
        public string Signature { get; set; } = string.Empty;

    }
    public class PayOSWebhookData 
    {
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string TransactionDateTime { get; set; } = string.Empty;
        public string Currency { get; set; } = "VND";
        public string PaymentLinkId { get; set; } = string.Empty;
        public string Code { get; set; } = "00";
        public string Desc { get; set; } = string.Empty;
        public string CounterAccountBankId { get; set; } = string.Empty;
        public string CounterAccountBankName { get; set; } = string.Empty;
        public string CounterAccountName { get; set; } = string.Empty;
        public string CounterAccountNumber { get; set; } = string.Empty;
        public string VirtualAccountName { get; set; } = string.Empty;
        public string VirtualAccountNumber { get; set; } = string.Empty;
    }
    public class PaymentHistoryDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string PlanId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? TransactionReference { get; set; }
    }
}
