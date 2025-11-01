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
    [Table("Quiz")]
    public class Quiz
    {
        [Key]
        public Guid Id { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string Title { get; set; } = "";

        [Column(TypeName = "varchar(255)")]
        public string? Description { get; set; }

        [Column(TypeName = "timestamp")]
        [DefaultValue("getdate()")]
        public DateTime CreateAt { get; set; } = DateTime.Now;

        [Column(TypeName = "timestamp")]
        public DateTime? StartTime { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? DueTime { get; set; }

        [Required, DefaultValue(1)]
        public int MaxTimesCanAttempt { get; set; } = 1;

        [DefaultValue(false)]
        public bool IsPublish { get; set; } = false;

        [DefaultValue(false)]
        public bool IsShuffleAnswers { get; set; } = false;

        [DefaultValue(false)]
        public bool IsShuffleQuestions { get; set; } = false;

        [ForeignKey("CreatorUser")]
        public Guid? CreatorId { get; set; }

        // Navigation
        public AppUser? CreatorUser { get; set; }
        public List<Question>? Questions { get; set; }
        public List<QuizParticipation>? Participations { get; set; }
    }
}