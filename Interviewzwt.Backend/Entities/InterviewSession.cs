using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Interviewzwt.Backend.Entities
{
    public class InterviewSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public Guid? JobRoleId { get; set; }

        [ForeignKey("JobRoleId")]
        public JobRole? JobRole { get; set; }

        public Guid? CustomJobId { get; set; }

        [ForeignKey("CustomJobId")]
        public CustomJobDescription? CustomJob { get; set; }

        public int? ResumeScore { get; set; }

        public string? ResumeFeedback { get; set; }

        public int? InterviewScore { get; set; }

        public string? InterviewFeedback { get; set; }

        public string? Transcript { get; set; }

        [Required]
        public string Status { get; set; } = "pending"; // pending, resume_uploaded, in_progress, completed

        public string? ResumePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
