using Interviewzwt.Backend.Entities;

namespace Interviewzwt.Backend.Interfaces
{
    public interface IInterviewService
    {
        Task<InterviewSession> CreateSession(Guid userId, Guid? jobRoleId, Guid? customJobId);
        Task<InterviewSession?> GetSessionById(Guid id);
        Task<InterviewSession> UpdateSession(Guid id, InterviewSession sessionUpdates);
        Task<IEnumerable<InterviewSession>> GetUserSessions(Guid userId);
    }
}
