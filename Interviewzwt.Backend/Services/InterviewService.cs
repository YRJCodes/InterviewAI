using Interviewzwt.Backend.Data;
using Interviewzwt.Backend.Entities;
using Interviewzwt.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Interviewzwt.Backend.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly ApplicationDbContext _context;

        public InterviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewSession> CreateSession(Guid userId, Guid? jobRoleId, Guid? customJobId)
        {
            var session = new InterviewSession
            {
                UserId = userId,
                JobRoleId = jobRoleId,
                CustomJobId = customJobId,
                Status = "pending"
            };

            _context.InterviewSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<InterviewSession?> GetSessionById(Guid id)
        {
            return await _context.InterviewSessions
                .Include(s => s.JobRole)
                .Include(s => s.CustomJob)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<InterviewSession> UpdateSession(Guid id, InterviewSession sessionUpdates)
        {
            var session = await _context.InterviewSessions.FindAsync(id);
            if (session == null) throw new Exception("Session not found");

            if (sessionUpdates.Status != null) session.Status = sessionUpdates.Status;
            if (sessionUpdates.ResumeScore != null) session.ResumeScore = sessionUpdates.ResumeScore;
            if (sessionUpdates.ResumeFeedback != null) session.ResumeFeedback = sessionUpdates.ResumeFeedback;
            if (sessionUpdates.InterviewScore != null) session.InterviewScore = sessionUpdates.InterviewScore;
            if (sessionUpdates.InterviewFeedback != null) session.InterviewFeedback = sessionUpdates.InterviewFeedback;
            if (sessionUpdates.Transcript != null) session.Transcript = sessionUpdates.Transcript;
            if (sessionUpdates.ResumePath != null) session.ResumePath = sessionUpdates.ResumePath;
            if (sessionUpdates.CompletedAt != null) session.CompletedAt = sessionUpdates.CompletedAt;

            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<IEnumerable<InterviewSession>> GetUserSessions(Guid userId)
        {
            return await _context.InterviewSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
