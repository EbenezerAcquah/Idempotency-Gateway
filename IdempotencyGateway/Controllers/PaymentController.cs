using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using IdempotencyGateway.Models;
using IdempotencyGateway.Services;

namespace IdempotencyGateway.Controllers
{
    [ApiController]
    [Route("process-payment")]      
    
    public class PaymentController(IdempotencyService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(
            [FromBody] PaymentRequest request)
        {
            if (!Request.Headers.TryGetValue("Idempotency-Key", out var keyValues) || string.IsNullOrEmpty(keyValues))
                return BadRequest("Missing Idempotency-Key");

            string key = keyValues.ToString();
            string requestHash = Hash(JsonSerializer.Serialize(request));

            var store = service.Store;

            if (store.TryGetValue(key, out var entry))
            {
                if (service.IsExpired(entry))
                {
                    store.TryRemove(key, out _);
                }
                else
                {
                    if (entry.RequestHash != requestHash)
                        return Conflict("Idempotency key already used for a different request body.");

                    if (entry.IsProcessing && entry.Waiter != null)
                    {
                        await entry.Waiter.Task;
                    }

                    Response.Headers["X-Cache-Hit"] = "true";
                    return Ok(entry.Response);
                }
            }

            var newEntry = new IdempotencyEntry
            {
                RequestHash = requestHash,
                IsProcessing = true,
                CreatedAt = DateTime.UtcNow,
                Waiter = new TaskCompletionSource<bool>()
            };

            store[key] = newEntry;

            await Task.Delay(2000);

            var response = new PaymentResponse
            {
                Message = $"Charged {request.Amount} {request.Currency}",
                ProcessedAt = DateTime.UtcNow
            };

            newEntry.Response = response;
            newEntry.IsProcessing = false;
            newEntry.Waiter?.SetResult(true);

            return Ok(response);
        }

        private static string Hash(string input)
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes);
        }
    }
}

