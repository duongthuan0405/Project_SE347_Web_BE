using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.UserProfile;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepo;
    
        public UserProfileService(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepo = userProfileRepository;
        }

        public async Task<UserProfileResponseDTO?> GetProfileByIdAsync(string id)
        {
            Guid guid = Guid.Parse(id);
            AppUserProfile? appUserProfile = await _userProfileRepo.GetProfileByIdAsync(guid);
            if (appUserProfile == null)
            {
                return null;
            }

            UserProfileResponseDTO userProfileDTO = new UserProfileResponseDTO()
            {
                Id = appUserProfile.Id.ToString(),
                FirstName = appUserProfile.FirstName,
                LastName = appUserProfile.LastName,
                Avatar = appUserProfile.Avatar,
            };

            return userProfileDTO;
        }

        public async Task<UserProfileResponseDTO?> UpdateProfileAsync(string id, UpdateUserProfileRequestDTO updateRequest, string? avatarURL)
        {
            AppUserProfile appUserProfile = new AppUserProfile()
            {
                Id = Guid.Parse(id),
                FirstName = updateRequest.FirstName ?? "",
                LastName = updateRequest.LastName ?? "",
                Avatar = avatarURL ?? ""
            };

            var result = await _userProfileRepo.UpdateUserProfileAsync(appUserProfile);
            return new UserProfileResponseDTO()
            {
                Id = result?.Id.ToString() ?? "",
                FirstName = result?.FirstName,
                LastName = result?.LastName,
                Avatar = result?.Avatar
            };
        }
    }
}