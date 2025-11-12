namespace se347_be.Work.DTOs.ParticipantList
{
    public class ParticipantDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? StudentId { get; set; }
        public string Email { get; set; } = null!;
    }
}
