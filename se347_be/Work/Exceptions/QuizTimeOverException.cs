using System;

namespace se347_be.Work.Exceptions
{
    public class QuizTimeOverException : Exception
    {
        public Guid ParticipationId { get; }

        public QuizTimeOverException(Guid participationId, string? message = null)
            : base(message ?? "Quiz time is over. Auto-submitted.")
        {
            ParticipationId = participationId;
        }
    }
}
