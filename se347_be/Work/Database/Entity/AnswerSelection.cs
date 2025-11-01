using System;
using System.ComponentModel.DataAnnotations.Schema;
using se347_be.Work.Database.Entity;

namespace se347_be.Work.Database.Entities
{
    [Table("AnswerSelection")]
    public class AnswerSelection
    {
        [ForeignKey("QuizParticipation")]
        public Guid ParticipationId { get; set; }

        [ForeignKey("Answer")]
        public Guid AnswerId { get; set; }

        // Navigation
        public QuizParticipation QuizParticipation { get; set; } = null!;
        public Answer Answer { get; set; } = null!;
    }
}
