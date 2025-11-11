using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.Answer
{
    public class CreateAnswerDTO
    {
        [Required(ErrorMessage = "Answer content is required")]
        [StringLength(200)]
        public string Content { get; set; } = "";

        [Required]
        public bool IsCorrectAnswer { get; set; } = false;
    }
}
