using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.Database.Entities;

namespace se347_be.Work.Database.Entity
{
    [Table("Answer")]
    public class Answer
    {
        [Key]
        public Guid Id { get; set; }

        [Required, Column(TypeName = "varchar(200)")]
        public string Content { get; set; } = null!;

        [ForeignKey("Question")]
        public Guid? QuestionId { get; set; }

        [Required, DefaultValue(false)]
        public bool IsCorrectAnswer { get; set; } = false;

        // Navigation
        public Question? Question { get; set; }
        public List<AnswerSelection>? AnswerSelections { get; set; }
    }
}