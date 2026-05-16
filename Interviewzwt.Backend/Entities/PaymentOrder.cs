using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Interviewzwt.Backend.Entities
{
    public class PaymentOrder
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int Credits { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
