using se347_be.Work.DTOs.AI;

namespace se347_be.Work.Services.Interfaces
{
    public interface IGeminiAIService
    {
        Task<GenerateQuizResponseDTO> GenerateQuestionsFromTextAsync(
            string textContent, 
            string fileName,
            int numberOfQuestions, 
            string? additionalInstructions = null);

        Task<GenerateAndSaveQuizResponseDTO> GenerateAndSaveQuestionsAsync(
 
            string textContent,
            string fileName,
            int numberOfQuestions,
            Guid creatorId,
            string category,
            string? additionalInstructions = null
            );
    }
}
