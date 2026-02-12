namespace IdempotencyGateway.Models
{
    //Defines the structure of the response returned to the client
    public class PaymentResponse
    {
        public required string Message { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}

