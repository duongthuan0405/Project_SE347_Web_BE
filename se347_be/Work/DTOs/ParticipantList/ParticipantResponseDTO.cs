namespace se347_be.Work.DTOs.ParticipantList
{
    public class ParticipantResponseDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? StudentId { get; set; }
    }
}
