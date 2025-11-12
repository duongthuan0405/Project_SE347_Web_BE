using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.QuestionBank
{
    public class CreateQuestionBankDTO
    {
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = null!;

        [Range(1, 100)]
        public int Points { get; set; } = 1;

        [StringLength(100)]
        public string? Category { get; set; }

        [Required]
        public List<CreateAnswerDTO> Answers { get; set; } = new();
    }

    public class CreateAnswerDTO
    {
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = null!;

        public bool IsCorrectAnswer { get; set; } = false;
    }
}
