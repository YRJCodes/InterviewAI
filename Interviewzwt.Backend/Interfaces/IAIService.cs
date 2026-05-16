using Interviewzwt.Backend.DTOs;

namespace Interviewzwt.Backend.Interfaces
{
    public interface IAIService
    {
        Task<ResumeAnalysisResponse> AnalyzeResume(string resumeText, string jobDescription, string jobTitle);
        Task<string> TranscribeAudio(string base64Audio);
        Task<string> GenerateInterviewResponse(string jobDescription, List<AIMessage> messages);
        Task<InterviewScoreResponse> ScoreInterview(string conversationText, string jobDescription);
    }

    public class ResumeAnalysisResponse
    {
        public int Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class InterviewScoreResponse
    {
        public int Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class AIMessage
    {
        public string Role { get; set; } = string.Empty; // user, system, assistant
        public string Content { get; set; } = string.Empty;
    }
}
