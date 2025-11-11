using se347_be.Work.DTOs.Participant;

namespace se347_be.Work.Services.Interfaces
{
    public interface IParticipantQuizService
    {
        Task<QuizInfoDTO> GetQuizInfoAsync(Guid quizId);
        Task<StartQuizResponseDTO> StartQuizAsync(Guid quizId, StartQuizRequestDTO dto);
        Task<QuizContentDTO> GetQuizContentAsync(Guid participationId);
        Task SaveAnswerAsync(Guid participationId, Guid questionId, Guid answerId);
        Task SaveAnswersAsync(Guid participationId, List<AnswerSubmissionDTO> answers);
        Task<SubmitQuizResponseDTO> SubmitQuizAsync(Guid participationId, SubmitQuizDTO dto);
        Task<SubmitQuizResponseDTO> GetParticipationResultAsync(Guid participationId);
    }
}
