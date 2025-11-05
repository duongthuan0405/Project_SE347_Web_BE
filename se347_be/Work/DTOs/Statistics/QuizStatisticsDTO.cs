namespace se347_be.Work.DTOs.Statistics
{
    public class QuizStatisticsDTO
    {
        public Guid QuizId { get; set; }
        public string Title { get; set; } = "";
        public int TotalParticipants { get; set; }
        public double AverageScore { get; set; }
        public Dictionary<string, int> ScoreDistribution { get; set; } = new();
    }
}
