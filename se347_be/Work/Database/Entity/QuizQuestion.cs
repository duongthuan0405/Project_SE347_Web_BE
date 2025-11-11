using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace se347_be.Work.Database.Entity
{
    [Table("QuizQuestion")]
    public class QuizQuestion
    {
        [Required]
        [ForeignKey("Quiz")]
        public Guid QuizId { get; set; }

        [Required]
        [ForeignKey("Question")]
        public Guid QuestionId { get; set; }

        [Required, DefaultValue(0)]
        public int OrderIndex { get; set; } = 0; // For fixed order when IsShuffleQuestions = false

        // Navigation
        public Quiz? Quiz { get; set; }
        public Question? Question { get; set; }
    }
}
