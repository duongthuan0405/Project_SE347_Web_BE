using ClosedXML.Excel;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.Statistics;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IParticipationRepository _participationRepository;

        public StatisticsService(
            IQuizRepository quizRepository,
            IParticipationRepository participationRepository)
        {
            _quizRepository = quizRepository;
            _participationRepository = participationRepository;
        }

        public async Task<QuizStatisticsDTO> GetQuizStatisticsAsync(Guid quizId, Guid creatorId)
        {
            // Verify ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to view statistics of this quiz");
            }

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            var participations = await _participationRepository.GetParticipationsByQuizIdAsync(quizId);

            // Use stored scores (already calculated correctly with points system)
            var scores = participations
                .Where(p => p.Score.HasValue)
                .Select(p => (double)p.Score!.Value)
                .ToList();

            var averageScore = scores.Any() ? scores.Average() : 0;

            // Calculate score distribution
            var distribution = new Dictionary<string, int>
            {
                { "0-2", scores.Count(s => s >= 0 && s < 2) },
                { "2-4", scores.Count(s => s >= 2 && s < 4) },
                { "4-6", scores.Count(s => s >= 4 && s < 6) },
                { "6-8", scores.Count(s => s >= 6 && s < 8) },
                { "8-10", scores.Count(s => s >= 8 && s <= 10) }
            };

            return new QuizStatisticsDTO
            {
                QuizId = quizId,
                Title = quiz.Title,
                TotalParticipants = participations.Count,
                AverageScore = Math.Round(averageScore, 2),
                ScoreDistribution = distribution
            };
        }

        public async Task<List<ParticipationListDTO>> GetParticipationsAsync(Guid quizId, Guid creatorId)
        {
            // Verify ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to view participations of this quiz");
            }

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            var participations = await _participationRepository.GetParticipationsByQuizIdAsync(quizId);

            var result = participations.Select(p => new ParticipationListDTO
            {
                ParticipationId = p.Id,
                FullName = p.FullName,
                StudentId = p.StudentId,
                ClassName = p.ClassName,
                Score = (double)(p.Score ?? 0),
                SubmitTime = p.SubmitTime
            })
            .OrderByDescending(p => p.Score)
            .ToList();

            return result;
        }

        public async Task<ParticipationDetailDTO> GetParticipationDetailAsync(Guid participationId, Guid creatorId)
        {
            var participation = await _participationRepository.GetParticipationWithDetailsAsync(participationId);

            if (participation == null)
            {
                throw new InvalidDataException("Participation not found");
            }

            if (participation.Quiz == null || participation.Quiz.CreatorId != creatorId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this participation");
            }

            var score = (double)(participation.Score ?? 0);
            var correctCount = 0;

            var details = new List<QuestionResultDTO>();
            var questions = participation.Quiz.QuizQuestions?
                .OrderBy(qq => qq.OrderIndex)
                .Select(qq => qq.Question!)
                .ToList() ?? new List<Question>();

            var totalQuestions = questions.Count;

            foreach (var question in questions)
            {
                var selectedAnswers = participation.AnswerSelections?
                    .Where(a => a.Answer.QuestionId == question.Id)
                    .Select(a => a.Answer)
                    .ToList();

                var correctAnswers = question.Answers?
                    .Where(a => a.IsCorrectAnswer)
                    .ToList();

                var selectedAnswer = selectedAnswers?.FirstOrDefault();
                var correctAnswer = correctAnswers?.FirstOrDefault();

                var isCorrect = selectedAnswer != null && selectedAnswer.IsCorrectAnswer;
                if (isCorrect) correctCount++;

                details.Add(new QuestionResultDTO
                {
                    QuestionContent = question.Content,
                    SelectedAnswer = selectedAnswer?.Content,
                    CorrectAnswer = correctAnswer?.Content ?? "",
                    IsCorrect = isCorrect
                });
            }

            return new ParticipationDetailDTO
            {
                ParticipationId = participation.Id,
                FullName = participation.FullName,
                StudentId = participation.StudentId,
                ClassName = participation.ClassName,
                Score = score,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctCount,
                SubmitTime = participation.SubmitTime,
                Details = details
            };
        }

        public async Task<byte[]> ExportToExcelAsync(Guid quizId, Guid creatorId)
        {
            var participations = await GetParticipationsAsync(quizId, creatorId);
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Results");

            // Headers
            worksheet.Cell(1, 1).Value = "STT";
            worksheet.Cell(1, 2).Value = "Họ Tên";
            worksheet.Cell(1, 3).Value = "MSSV";
            worksheet.Cell(1, 4).Value = "Lớp";
            worksheet.Cell(1, 5).Value = "Điểm";
            worksheet.Cell(1, 6).Value = "Thời gian nộp";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data rows
            int row = 2;
            int stt = 1;
            foreach (var participation in participations)
            {
                worksheet.Cell(row, 1).Value = stt++;
                worksheet.Cell(row, 2).Value = participation.FullName ?? "";
                worksheet.Cell(row, 3).Value = participation.StudentId ?? "";
                worksheet.Cell(row, 4).Value = participation.ClassName ?? "";
                worksheet.Cell(row, 5).Value = participation.Score;
                worksheet.Cell(row, 6).Value = participation.SubmitTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Convert to byte array
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
}
