using Interviewzwt.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/functions")]
    [ApiController]
    [Authorize]
    public class FunctionsController : ControllerBase
    {
        private readonly IAIService _aiService;

        public FunctionsController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("analyze-resume")]
        public async Task<IActionResult> AnalyzeResume([FromBody] AnalyzeResumeRequest request)
        {
            var result = await _aiService.AnalyzeResume(request.ResumeText, request.JobDescription, request.JobTitle);
            return Ok(result);
        }

        [HttpPost("score-interview")]
        public async Task<IActionResult> ScoreInterview([FromBody] ScoreInterviewRequest request)
        {
            var result = await _aiService.ScoreInterview(request.ConversationText, request.JobDescription);
            return Ok(result);
        }

        [HttpPost("voice-interview")]
        public async Task<IActionResult> VoiceInterview([FromBody] VoiceInterviewRequest request)
        {
            if (request.Action == "transcribe")
            {
                if (string.IsNullOrEmpty(request.AudioData)) return BadRequest(new { error = "Audio data required" });
                var transcript = await _aiService.TranscribeAudio(request.AudioData);
                return Ok(new { transcript });
            }
            else if (request.Action == "generate")
            {
                var message = await _aiService.GenerateInterviewResponse(request.JobDescription, request.Messages);
                return Ok(new { message });
            }

            return BadRequest(new { error = "Invalid action" });
        }
    }

    public class AnalyzeResumeRequest
    {
        public string ResumeText { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }

    public class ScoreInterviewRequest
    {
        public string ConversationText { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
    }

    public class VoiceInterviewRequest
    {
        public string Action { get; set; } = string.Empty; // transcribe, generate
        public string? AudioData { get; set; }
        public string JobDescription { get; set; } = string.Empty;
        public List<AIMessage> Messages { get; set; } = new List<AIMessage>();
    }
}
