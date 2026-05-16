namespace Interviewzwt.Backend.Interfaces
{
    public interface IFileService
    {
        Task<string> ExtractTextFromFile(IFormFile file);
    }
}
