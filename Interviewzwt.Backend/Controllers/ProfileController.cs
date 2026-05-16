using Interviewzwt.Backend.Data;
using Interviewzwt.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (idClaim == null) return Unauthorized(new { message = "ID claim not found" });

                if (!Guid.TryParse(idClaim.Value, out var userId)) 
                    return BadRequest(new { message = "Invalid ID claim format" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound(new { message = "User not found" });

                return Ok(new { user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.Credits
                }});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPatch("credits")]
        public async Task<IActionResult> UpdateCredits([FromBody] UpdateCreditsRequest request)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Credits += request.Credits;
            await _context.SaveChangesAsync();

            return Ok(new { credits = user.Credits });
        }
    }

    public class UpdateCreditsRequest
    {
        public int Credits { get; set; }
    }
}
