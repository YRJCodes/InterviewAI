using Interviewzwt.Backend.DTOs;
using Interviewzwt.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/custom-job")]
    [ApiController]
    [Authorize]
    public class CustomJobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public CustomJobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomJobRequest request)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            var customJob = await _jobService.CreateCustomJob(userId, request.Title, request.Description, request.Requirements);
            return Ok(customJob);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var job = await _jobService.GetCustomJobById(id);
            if (job == null) return NotFound();
            return Ok(job);
        }
    }

    public class CustomJobRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Requirements { get; set; } = new List<string>();
    }
}
