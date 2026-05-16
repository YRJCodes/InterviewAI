using Interviewzwt.Backend.DTOs;
using Interviewzwt.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request);
            if (result == null)
                return BadRequest(new { message = "Email already exists" });

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.Login(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("test-claims")]
        public IActionResult TestClaims()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            try
            {
                // Try to find the "id" claim, falling back to NameIdentifier if needed
                var userIdClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null) 
                    return Unauthorized(new { message = "User ID claim not found in token" });

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return BadRequest(new { message = "Invalid User ID format in token" });

                var user = await _authService.GetUserById(userId);

                if (user == null) 
                    return NotFound(new { message = "User not found in database" });

                return Ok(new { user = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Credits = user.Credits
                }});
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app) and return a 500 with details for debugging
                return StatusCode(500, new { 
                    message = "An error occurred while retrieving user info", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }
    }
}
