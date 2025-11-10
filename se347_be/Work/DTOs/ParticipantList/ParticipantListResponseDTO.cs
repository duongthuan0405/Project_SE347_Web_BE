namespace se347_be.Work.DTOs.ParticipantList
{
    public class ParticipantListResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid CreatorId { get; set; }
        public int ParticipantCount { get; set; }
    }
}
