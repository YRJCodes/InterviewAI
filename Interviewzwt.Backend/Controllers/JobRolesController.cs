using Interviewzwt.Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/job-roles")]
    [ApiController]
    public class JobRolesController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobRolesController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _jobService.GetAllJobRoles();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await _jobService.GetJobRoleById(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            await _jobService.SeedJobRoles();
            return Ok(new { message = "Job roles seeded successfully" });
        }
    }
}
