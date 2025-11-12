using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace se347_be.Work.DTOs.AI
{
    public class GenerateQuizFormDTO
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Range(1, 100, ErrorMessage = "Number of questions must be between 1 and 100")]
        public int NumberOfQuestions { get; set; } = 10;

        public string? AdditionalInstructions { get; set; }
    }
}
