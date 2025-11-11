using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.AI
{
    public class GenerateQuizRequestDTO
    {
        [Range(1, 100, ErrorMessage = "Number of questions must be between 1 and 100")]
        public int NumberOfQuestions { get; set; } = 10;

        public string? AdditionalInstructions { get; set; }
    }
}
