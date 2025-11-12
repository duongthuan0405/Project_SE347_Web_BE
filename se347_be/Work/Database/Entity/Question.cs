using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Work.Database.Entity
{
    [Table("Question")]
    public class Question
    {
        [Key]
        public Guid Id { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string Content { get; set; } = null!;

        [Required, DefaultValue(1)]
        public int Points { get; set; } = 1;

        [ForeignKey("CreatorUser")]
        public Guid? CreatorId { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Category { get; set; } // Free-text category for filtering

        [DefaultValue(false)]
        public bool IsDraft { get; set; } = false; // For AI-generated questions pending review

        // Navigation
        public AppUser? CreatorUser { get; set; }
        public List<Answer>? Answers { get; set; }
        public List<QuizQuestion>? QuizQuestions { get; set; } // Many-to-many with Quiz
    }
}