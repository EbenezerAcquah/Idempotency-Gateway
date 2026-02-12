namespace IdempotencyGateway.Models
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
    }
}
