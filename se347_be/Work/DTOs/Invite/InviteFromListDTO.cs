using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.Invite
{
    public class InviteFromListDTO
    {
        [Required]
        public List<Guid> ParticipantListIds { get; set; } = new();
    }
}
