using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.Quiz
{
    public class CreateQuizDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255)]
        public string Title { get; set; } = "";

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? DueTime { get; set; }

        public int MaxTimesCanAttempt { get; set; } = 1;

        public bool IsPublish { get; set; } = false;

        public bool IsShuffleAnswers { get; set; } = false;

        public bool IsShuffleQuestions { get; set; } = false;
    }
}
