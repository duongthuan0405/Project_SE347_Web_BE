namespace se347_be.Work.DTOs.Invite
{
    public class InviteResponseDTO
    {
        public int TotalSent { get; set; }
        public List<string> Failed { get; set; } = new();
    }
}
