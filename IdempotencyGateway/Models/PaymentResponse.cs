namespace IdempotencyGateway.Models
{
    public class PaymentResponse
    {
        public required string Message { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}

