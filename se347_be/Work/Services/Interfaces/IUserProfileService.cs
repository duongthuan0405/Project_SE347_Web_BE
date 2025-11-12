using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.DTOs.UserProfile;

namespace se347_be.Work.Services.Interfaces
{
    public interface IUserProfileService
    {
        public Task<UserProfileResponseDTO?> UpdateProfileAsync(string id, UpdateUserProfileRequestDTO updateRequest, IFormFile? avatar);
        public Task<UserProfileResponseDTO?> GetProfileByIdAsync(string id);
    }
}