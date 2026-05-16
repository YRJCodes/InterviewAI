using Interviewzwt.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interviewzwt.Backend.Controllers
{
    [Route("api/upload")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public UploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });

            try
            {
                var extractedText = await _fileService.ExtractTextFromFile(file);
                
                if (string.IsNullOrEmpty(extractedText))
                {
                    return BadRequest(new { message = "Could not extract text from file" });
                }

                return Ok(new
                {
                    fileName = file.FileName,
                    extractedText = extractedText,
                    extractedTextLength = extractedText.Length
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
