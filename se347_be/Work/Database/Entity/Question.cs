using System;
using System.Collections.Generic;
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

        [ForeignKey("Quiz")]
        public Guid? QuizId { get; set; }

        // Navigation
        public Quiz? Quiz { get; set; }
        public List<Answer>? Answers { get; set; }
    }
}