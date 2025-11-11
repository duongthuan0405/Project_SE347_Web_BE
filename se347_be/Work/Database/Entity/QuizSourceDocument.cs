using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace se347_be.Work.Database.Entity
{
    [Table("QuizSourceDocument")]
    public class QuizSourceDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, ForeignKey("Quiz")]
        public Guid QuizId { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string FileName { get; set; } = null!;

        [Required, Column(TypeName = "varchar(500)")]
        public string StorageUrl { get; set; } = null!;

        [Required, Column(TypeName = "varchar(50)")]
        [DefaultValue("Uploaded")]
        public string Status { get; set; } = "Uploaded"; // Uploaded, Processing, Completed, Failed

        [Column(TypeName = "timestamptz")]
        [DefaultValue("now()")]
        public DateTime? UploadAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Quiz? Quiz { get; set; }
    }
}
