using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.Quiz
{
    public class UpdateQuizDTO
    {
        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? DueTime { get; set; }

        public int? MaxTimesCanAttempt { get; set; }

        public bool? IsPublish { get; set; }

        public bool? IsShuffleAnswers { get; set; }

        public bool? IsShuffleQuestions { get; set; }
    }
}
