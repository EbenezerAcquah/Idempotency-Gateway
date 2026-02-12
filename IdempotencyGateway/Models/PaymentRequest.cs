namespace IdempotencyGateway.Models
{
    //Defines the data the API expects when a client sends a payment
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
    }
}
