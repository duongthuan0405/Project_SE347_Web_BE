using se347_be.Work.DTOs.UserProfile;

namespace se347_be.Work.DTOs
{
    public class SignInResponseDTO
    {
        public string Token { get; set; } = "";
        public UserProfileFullDTO? UserFullProfile { get; set; }
    }
}
