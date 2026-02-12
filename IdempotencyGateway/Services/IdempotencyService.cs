using System.Collections.Concurrent;

namespace IdempotencyGateway.Services
{
    public class IdempotencyService
    {
        private readonly ConcurrentDictionary<string, IdempotencyEntry> _store = new();

        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

        public ConcurrentDictionary<string, IdempotencyEntry> Store => _store;

        public bool IsExpired(IdempotencyEntry entry)
        {
            return DateTime.UtcNow - entry.CreatedAt > _ttl;
        }
    }
}

