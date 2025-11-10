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

        public int? DurationInMinutes { get; set; }

        [StringLength(50)]
        public string? AccessCode { get; set; }

        public bool? ShowScoreAfterSubmission { get; set; }

        public bool? SendResultEmail { get; set; }

        [StringLength(20)]
        public string? ShowCorrectAnswersMode { get; set; } // "Never", "Immediately", "AfterDueTime"

        public bool? AllowNavigationBack { get; set; }

        [StringLength(20)]
        public string? PresentationMode { get; set; } // "AllAtOnce", "OneByOne"
    }
}
