using Interviewzwt.Backend.Data;
using Interviewzwt.Backend.Entities;
using Interviewzwt.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Interviewzwt.Backend.Services
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;

        public JobService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobRole>> GetAllJobRoles()
        {
            return await _context.JobRoles.ToListAsync();
        }

        public async Task<JobRole?> GetJobRoleById(Guid id)
        {
            return await _context.JobRoles.FindAsync(id);
        }

        public async Task<CustomJobDescription?> GetCustomJobById(Guid id)
        {
            return await _context.CustomJobs.FindAsync(id);
        }

        public async Task<IEnumerable<CustomJobDescription>> GetCustomJobsByUser(Guid userId)
        {
            return await _context.CustomJobs
                .Where(job => job.UserId == userId)
                .OrderByDescending(job => job.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteCustomJob(Guid userId, Guid customJobId)
        {
            var customJob = await _context.CustomJobs.FirstOrDefaultAsync(job => job.Id == customJobId && job.UserId == userId);
            if (customJob == null)
            {
                return false;
            }

            _context.CustomJobs.Remove(customJob);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CustomJobDescription> CreateCustomJob(Guid userId, string title, string description, List<string> requirements)
        {
            var customJob = new CustomJobDescription
            {
                UserId = userId,
                Title = title,
                Description = description,
                Requirements = requirements
            };

            _context.CustomJobs.Add(customJob);
            await _context.SaveChangesAsync();
            return customJob;
        }

        public async Task SeedJobRoles()
        {
            if (await _context.JobRoles.AnyAsync()) return;

            var jobRoles = new List<JobRole>
            {
                new JobRole
                {
                    Title = "Software Engineer",
                    Description = "Develops and maintains software applications.",
                    Category = "Technology",
                    Requirements = new List<string> { "Knowledge of C#", "Problem-solving skills" },
                    Skills = new List<string> { "C#", "ASP.NET Core" },
                    Icon = "code"
                },
                new JobRole
                {
                    Title = "Frontend Developer",
                    Description = "Builds user interfaces and interactive web applications.",
                    Category = "Technology",
                    Requirements = new List<string> { "HTML, CSS, JavaScript", "UI/UX understanding" },
                    Skills = new List<string> { "React", "TypeScript", "JavaScript" },
                    Icon = "layout"
                },
                new JobRole
                {
                    Title = "Backend Developer",
                    Description = "Designs and develops server-side logic and APIs.",
                    Category = "Technology",
                    Requirements = new List<string> { "API design knowledge", "Database understanding" },
                    Skills = new List<string> { "ASP.NET Core", "SQL", "REST APIs" },
                    Icon = "server"
                },
                new JobRole
                {
                    Title = "DevOps Engineer",
                    Description = "Manages deployment pipelines, cloud infrastructure, and CI/CD.",
                    Category = "Technology",
                    Requirements = new List<string> { "Cloud platforms knowledge", "CI/CD experience" },
                    Skills = new List<string> { "Docker", "Kubernetes", "Azure/AWS" },
                    Icon = "cloud"
                },
                new JobRole
                {
                    Title = "UI/UX Designer",
                    Description = "Designs user-friendly interfaces and improves user experience.",
                    Category = "Design",
                    Requirements = new List<string> { "Design thinking", "User research" },
                    Skills = new List<string> { "Figma", "Wireframing", "Prototyping" },
                    Icon = "palette"
                },
                new JobRole
                {
                    Title = "QA Engineer",
                    Description = "Ensures software quality through testing and automation.",
                    Category = "Technology",
                    Requirements = new List<string> { "Attention to detail", "Testing knowledge" },
                    Skills = new List<string> { "Selenium", "Manual Testing", "Automation" },
                    Icon = "bug"
                },
                new JobRole
                {
                    Title = "Machine Learning Engineer",
                    Description = "Builds and deploys machine learning models.",
                    Category = "AI/ML",
                    Requirements = new List<string> { "Strong math background", "ML fundamentals" },
                    Skills = new List<string> { "Python", "TensorFlow", "Scikit-learn" },
                    Icon = "brain"
                },
                new JobRole
                {
                    Title = "Data Analyst",
                    Description = "Analyzes data to provide insights.",
                    Category = "Technology",
                    Requirements = new List<string> { "Strong SQL", "Statistics" },
                    Skills = new List<string> { "SQL", "Python" },
                    Icon = "bar-chart"
                },
                new JobRole
                {
                    Title = "Project Manager",
                    Description = "Manages projects and teams.",
                    Category = "Business",
                    Requirements = new List<string> { "Communication", "Leadership" },
                    Skills = new List<string> { "Agile", "Scrum" },
                    Icon = "users"
                }
            };

            _context.JobRoles.AddRange(jobRoles);
            await _context.SaveChangesAsync();
        }
    }
}
