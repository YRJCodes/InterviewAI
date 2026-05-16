using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Interviewzwt.Backend.Entities
{
    public class JobRole
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public List<string> Requirements { get; set; } = new List<string>();

        public List<string> Skills { get; set; } = new List<string>();

        public string? Icon { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
