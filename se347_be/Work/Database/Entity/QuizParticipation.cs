using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using se347_be.Work.Database.Entity;

namespace se347_be.Work.Database.Entities
{
    [Table("QuizParticipation")]
    public class QuizParticipation
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Quiz")]
        public Guid? QuizId { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? StudentId { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? ClassName { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? FullName { get; set; }

        [Required, Column(TypeName = "timestamp")]
        [DefaultValue("getdate()")]
        public DateTime ParticipationTime { get; set; } = DateTime.Now;

        [Column(TypeName = "timestamp")]
        public DateTime? SubmitTime { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? Email { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Score { get; set; }

        [Column(TypeName = "text")]
        public string? ShuffledQuestionsJson { get; set; } // JSON array: ["qId1", "qId2", ...] - Cleared after submit

        [Column(TypeName = "text")]
        public string? ShuffledAnswersJson { get; set; } // JSON map: {"qId1": ["aId1", "aId2"], ...} - Cleared after submit

        // Navigation
        public Quiz? Quiz { get; set; }
        public ICollection<AnswerSelection>? AnswerSelections { get; set; }
    }
}
