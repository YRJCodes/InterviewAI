using Interviewzwt.Backend.Entities;
using Interviewzwt.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/interview-sessions")]
    [ApiController]
    [Authorize]
    public class InterviewSessionsController : ControllerBase
    {
        private readonly IInterviewService _interviewService;

        public InterviewSessionsController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            var session = await _interviewService.CreateSession(userId, request.JobRoleId, request.CustomJobId);
            return Ok(session);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var session = await _interviewService.GetSessionById(id);
            if (session == null) return NotFound();
            return Ok(session);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InterviewSession sessionUpdates)
        {
            try
            {
                var session = await _interviewService.UpdateSession(id, sessionUpdates);
                return Ok(session);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSessions()
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            var sessions = await _interviewService.GetUserSessions(userId);
            return Ok(sessions);
        }
    }

    public class CreateSessionRequest
    {
        public Guid? JobRoleId { get; set; }
        public Guid? CustomJobId { get; set; }
    }
}
