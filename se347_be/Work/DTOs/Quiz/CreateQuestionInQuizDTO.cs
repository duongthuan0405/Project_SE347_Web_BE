using System.ComponentModel.DataAnnotations;
using se347_be.Work.DTOs.QuestionBank;

namespace se347_be.Work.DTOs.Quiz
{
    public class CreateQuestionInQuizDTO
    {
        public string? Id { get; set; } = null;
        [Required(ErrorMessage = "Content is required")]
        [StringLength(255)]
        public string Content { get; set; } = "";

        [Range(1, 100)]
        public int Points { get; set; } = 1;

        

        public bool IsDraft { get; set; } = false;

        [Required(ErrorMessage = "At least 2 answers are required")]
        [MinLength(2, ErrorMessage = "At least 2 answers are required")]
        public List<CreateAnswerDTO> Answers { get; set; } = new();
    }
}
