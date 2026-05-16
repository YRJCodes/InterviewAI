using Interviewzwt.Backend.Entities;

namespace Interviewzwt.Backend.Interfaces
{
    public interface IJobService
    {
        Task<IEnumerable<JobRole>> GetAllJobRoles();
        Task<JobRole?> GetJobRoleById(Guid id);
        Task<CustomJobDescription?> GetCustomJobById(Guid id);
        Task<CustomJobDescription> CreateCustomJob(Guid userId, string title, string description, List<string> requirements);
        Task SeedJobRoles();
    }
}
