using Interviewzwt.Backend.Data;
using Interviewzwt.Backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentsController(ApplicationDbContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [Authorize]
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var token = await GetPayPalToken();
                var baseUrl = _configuration["ExternalAPIs:PayPal:Mode"] == "production" 
                    ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";

                var payload = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            amount = new { currency_code = "USD", value = request.Amount.ToString("F2") },
                            custom_id = request.Credits.ToString(),
                            description = $"Purchase {request.Credits} interview credits"
                        }
                    },
                    application_context = new
                    {
                        return_url = "http://localhost:8080/payments/return",
                        cancel_url = "http://localhost:8080/pricing",
                        user_action = "PAY_NOW"
                    }
                };

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(content);
                var orderId = data.RootElement.GetProperty("id").GetString();
                
                var approveUrl = "";
                foreach (var link in data.RootElement.GetProperty("links").EnumerateArray())
                {
                    if (link.GetProperty("rel").GetString() == "approve")
                    {
                        approveUrl = link.GetProperty("href").GetString();
                        break;
                    }
                }

                // Persist mapping
                var userId = Guid.Parse(User.FindFirst("id")!.Value);
                var paymentOrder = new PaymentOrder
                {
                    OrderId = orderId!,
                    UserId = userId,
                    Credits = request.Credits
                };
                _context.PaymentOrders.Add(paymentOrder);
                await _context.SaveChangesAsync();

                return Ok(new { orderID = orderId, approveUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create PayPal order", error = ex.Message });
            }
        }

        [HttpPost("capture/{orderId}")]
        public async Task<IActionResult> CaptureOrder(string orderId)
        {
            try
            {
                var token = await GetPayPalToken();
                var baseUrl = _configuration["ExternalAPIs:PayPal:Mode"] == "production" 
                    ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{orderId}/capture");
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                httpRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                
                int credits = 0;
                Guid? userId = null;

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(content);
                    var pu = data.RootElement.GetProperty("purchase_units")[0];
                    if (pu.TryGetProperty("custom_id", out var customId))
                    {
                        credits = int.Parse(customId.GetString()!);
                    }
                }
                else
                {
                    // Fallback to mapping
                    var mapping = await _context.PaymentOrders.FirstOrDefaultAsync(p => p.OrderId == orderId);
                    if (mapping != null)
                    {
                        credits = mapping.Credits;
                        userId = mapping.UserId;
                    }
                    else
                    {
                        return BadRequest(new { message = "Order not found and capture failed" });
                    }
                }

                if (userId == null)
                {
                    // If not found in mapping fallback, try to get from mapping anyway
                    var mapping = await _context.PaymentOrders.FirstOrDefaultAsync(p => p.OrderId == orderId);
                    if (mapping != null)
                    {
                        userId = mapping.UserId;
                        credits = mapping.Credits;
                    }
                }

                if (userId != null && credits > 0)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.Credits += credits;
                        await _context.SaveChangesAsync();
                        
                        // Cleanup
                        var mapping = await _context.PaymentOrders.FirstOrDefaultAsync(p => p.OrderId == orderId);
                        if (mapping != null) _context.PaymentOrders.Remove(mapping);
                        await _context.SaveChangesAsync();

                        return Ok(new { captured = true, credits = user.Credits });
                    }
                }

                return Ok(new { captured = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to capture PayPal order", error = ex.Message });
            }
        }

        private async Task<string> GetPayPalToken()
        {
            var clientId = _configuration["ExternalAPIs:PayPal:ClientId"];
            var clientSecret = _configuration["ExternalAPIs:PayPal:ClientSecret"];
            var baseUrl = _configuration["ExternalAPIs:PayPal:Mode"] == "production" 
                ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonDocument.Parse(content);
            return data.RootElement.GetProperty("access_token").GetString()!;
        }
    }

    public class CreateOrderRequest
    {
        public double Amount { get; set; }
        public int Credits { get; set; }
        public string? PlanName { get; set; }
    }
}
