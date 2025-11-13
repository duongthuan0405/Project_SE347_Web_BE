using se347_be.Work.DTOs.User;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;
using System.Threading.Tasks;

namespace se347_be.Work.Services.Implementations
{
    
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository) 
        { 
            _userRepository = userRepository;
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string id)
        {
            Guid guid;
            if(!Guid.TryParse(id, out guid))
            {
                throw new ArgumentException("GUID of user is invalid!");
            }
            var user = await _userRepository.GetUserByIdAsync(guid);
            if (user == null) {
                return null;
            }

            return new UserResponseDTO
            {
                Id = user.Id.ToString(),
                Email = user.Email,
            };

        }
    }
}
