using IdempotencyGateway.Models;

namespace IdempotencyGateway.Services
{
    public class IdempotencyEntry
    {
        public required string RequestHash { get; set; }
        public PaymentResponse? Response { get; set; }
        public bool IsProcessing { get; set; }
        public DateTime CreatedAt { get; set; }
        public TaskCompletionSource<bool>? Waiter { get; set; }
    }
}
