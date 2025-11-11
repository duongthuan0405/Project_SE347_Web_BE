using System.ComponentModel.DataAnnotations;
using se347_be.Work.DTOs.Answer;

namespace se347_be.Work.DTOs.Question
{
    public class CreateQuestionDTO
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(255)]
        public string Content { get; set; } = "";

        [Required(ErrorMessage = "At least one answer is required")]
        [MinLength(2, ErrorMessage = "At least 2 answers are required")]
        public List<CreateAnswerDTO> Answers { get; set; } = new();
    }
}
