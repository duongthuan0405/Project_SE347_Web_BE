using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace se347_be.Work.Database.Entity
{
    [Table("QuizInvitation")]
    public class QuizInvitation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Quiz")]
        public Guid QuizId { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string Email { get; set; } = null!;

        [Column(TypeName = "varchar(100)")]
        public string? StudentId { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? FullName { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime InvitedAt { get; set; } = DateTime.Now;

        // Navigation
        public Quiz? Quiz { get; set; }
    }
}
