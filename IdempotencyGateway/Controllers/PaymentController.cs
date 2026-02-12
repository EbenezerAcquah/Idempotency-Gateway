using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using IdempotencyGateway.Models;
using IdempotencyGateway.Services;

namespace IdempotencyGateway.Controllers
{
    [ApiController]
    [Route("[controller]")] // this will make the route /Payment or use "process-payment"
    public class PaymentController(IdempotencyService service) : ControllerBase
    {
        [HttpPost("/process-payment")] 
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            //check idempotency key
            if (!Request.Headers.TryGetValue("Idempotency-Key", out var keyValues) || string.IsNullOrEmpty(keyValues))
                return BadRequest("Missing Idempotency-Key");

            string key = keyValues.ToString();
            
            //key cannot be used with different request body
            string requestHash = Hash(JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

            var store = service.Store;

            
            if (store.TryGetValue(key, out var entry))
            {
                if (service.IsExpired(entry))              // If stored entry expired, remove it
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

                    Response.Headers.Append("X-Cache-Hit", "true");
                    return Ok(entry.Response);
                }
            }

            var newEntry = new IdempotencyEntry
            {
                RequestHash = requestHash,
                IsProcessing = true,
                CreatedAt = DateTime.UtcNow,

                // Allows duplicate requests to wait until processing completes
                Waiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
            };

            if (!store.TryAdd(key, newEntry)) 
            {
                
                return await ProcessPayment(request);
            }

            try 
            {
                await Task.Delay(2000);        // simulate external API or DB processing

                var response = new PaymentResponse
                {
                    Message = $"Charged {request.Amount} {request.Currency}",
                    ProcessedAt = DateTime.UtcNow
                };
                
                // Save result for reuse
                newEntry.Response = response;
                return Ok(response);
            }
            finally 
            {
                newEntry.IsProcessing = false;
                newEntry.Waiter?.TrySetResult(true);
            }
        }

        // Converts input string to SHA256 hash
        private static string Hash(string input)
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes);
        }
    }
}