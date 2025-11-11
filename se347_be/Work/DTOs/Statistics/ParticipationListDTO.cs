namespace se347_be.Work.DTOs.Statistics
{
    public class ParticipationListDTO
    {
        public Guid ParticipationId { get; set; }
        public string? FullName { get; set; }
        public string? StudentId { get; set; }
        public string? ClassName { get; set; }
        public double Score { get; set; }
        public DateTime? SubmitTime { get; set; }
    }
}
