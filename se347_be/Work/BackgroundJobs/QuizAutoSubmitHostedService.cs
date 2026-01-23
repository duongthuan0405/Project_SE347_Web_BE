using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using se347_be.Database;
using se347_be.Work.DTOs.Participant;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.BackgroundJobs
{
    public class QuizAutoSubmitHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QuizAutoSubmitHostedService> _logger;

        public QuizAutoSubmitHostedService(IServiceScopeFactory scopeFactory, ILogger<QuizAutoSubmitHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Quiz auto-submit job failed");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private static DateTime? GetEffectiveDeadlineUtc(DateTime participationTimeUtc, int? durationInMinutes, DateTime? dueTimeUtc)
        {
            DateTime? durationDeadlineUtc = durationInMinutes.HasValue
                ? participationTimeUtc.AddMinutes(durationInMinutes.Value)
                : (DateTime?)null;

            if (durationDeadlineUtc.HasValue && dueTimeUtc.HasValue)
            {
                return durationDeadlineUtc.Value <= dueTimeUtc.Value ? durationDeadlineUtc.Value : dueTimeUtc.Value;
            }

            return durationDeadlineUtc ?? dueTimeUtc;
        }

        private async Task RunOnceAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
            var participantQuizService = scope.ServiceProvider.GetRequiredService<IParticipantQuizService>();

            var openParticipations = await db.QuizParticipations
                .AsNoTracking()
                .Where(p => p.SubmitTime == null && p.QuizId != null)
                .Select(p => new { p.Id, p.QuizId, p.ParticipationTime })
                .ToListAsync(stoppingToken);

            if (openParticipations.Count == 0)
            {
                return;
            }

            var quizIds = openParticipations.Select(p => p.QuizId!.Value).Distinct().ToList();
            var quizMeta = await db.Quizzes
                .AsNoTracking()
                .Where(q => quizIds.Contains(q.Id))
                .Select(q => new { q.Id, q.DueTime, q.DurationInMinutes })
                .ToListAsync(stoppingToken);

            var quizMap = quizMeta.ToDictionary(x => x.Id, x => x);
            var now = DateTime.UtcNow;

            foreach (var p in openParticipations)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                if (!quizMap.TryGetValue(p.QuizId!.Value, out var q))
                {
                    continue;
                }

                var deadline = GetEffectiveDeadlineUtc(p.ParticipationTime, q.DurationInMinutes, q.DueTime);
                if (!deadline.HasValue)
                {
                    continue;
                }

                if (now < deadline.Value)
                {
                    continue;
                }

                try
                {
                    await participantQuizService.SubmitQuizAsync(p.Id, new SubmitQuizDTO());
                }
                catch (InvalidDataException)
                {
                    // Already submitted or participation not found; safe to ignore
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to auto-submit participation {ParticipationId}", p.Id);
                }
            }
        }
    }
}
