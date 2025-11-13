using se347_be.Work.DTOs.User;

namespace se347_be.Work.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserResponseDTO?> GetUserByIdAsync(string id);
    }
}
