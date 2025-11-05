using se347_be.Work.DTOs.Quiz;

namespace se347_be.Work.Services.Interfaces
{
    public interface IQuizService
    {
        Task<QuizResponseDTO> CreateQuizAsync(CreateQuizDTO createQuizDTO, Guid creatorId);
        Task<List<QuizResponseDTO>> GetQuizzesByCreatorAsync(Guid creatorId);
        Task<QuizDetailDTO> GetQuizDetailAsync(Guid quizId, Guid creatorId);
        Task<QuizResponseDTO> UpdateQuizAsync(Guid quizId, UpdateQuizDTO updateQuizDTO, Guid creatorId);
        Task DeleteQuizAsync(Guid quizId, Guid creatorId);
    }
}
