using System.Collections.Concurrent;

namespace IdempotencyGateway.Services
{
    //Stores request records and helps check if they are still valid
    public class IdempotencyService
    {
        private readonly ConcurrentDictionary<string, IdempotencyEntry> _store = new();

        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(10);

        public ConcurrentDictionary<string, IdempotencyEntry> Store => _store;

        public bool IsExpired(IdempotencyEntry entry)
        {
            return DateTime.UtcNow - entry.CreatedAt > _ttl;
        }
    }
}

