using Interviewzwt.Backend.Entities;

namespace Interviewzwt.Backend.Interfaces
{
    public interface IJobService
    {
        Task<IEnumerable<JobRole>> GetAllJobRoles();
            Task<JobRole?> GetJobRoleById(Guid id);
        Task<CustomJobDescription?> GetCustomJobById(Guid id);
        Task<IEnumerable<CustomJobDescription>> GetCustomJobsByUser(Guid userId);
        Task<bool> DeleteCustomJob(Guid userId, Guid customJobId);
        Task<CustomJobDescription> CreateCustomJob(Guid userId, string title, string description, List<string> requirements);
        Task SeedJobRoles();
    }
}
