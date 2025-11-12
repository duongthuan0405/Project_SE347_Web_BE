namespace se347_be.Work.DTOs.ParticipantList
{
    public class ParticipantListDetailDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid CreatorId { get; set; }
        public List<ParticipantDTO> Participants { get; set; } = new();
    }
}
