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

        public async Task<UserProfileDTO?> GetProfileByIdAsync(string id)
        {
            Guid guid = Guid.Parse(id);
            AppUserProfile? appUserProfile = await _userProfileRepo.GetProfileByIdAsync(guid);
            if (appUserProfile == null)
            {
                return null;
            }

            UserProfileDTO userProfileDTO = new UserProfileDTO()
            {
                Id = appUserProfile.Id.ToString(),
                FirstName = appUserProfile.FirstName,
                LastName = appUserProfile.LastName,
                Avatar = appUserProfile.Avatar,
            };

            return userProfileDTO;
        }

        public async Task<UserProfileDTO?> UpdateProfileAsync(UserProfileDTO userProfileDTO)
        {
            AppUserProfile? profileExistInDb = await _userProfileRepo.GetProfileByIdAsync(Guid.Parse(userProfileDTO.Id));
            if (profileExistInDb == null)
            {
                return null;
            }

            AppUserProfile appUserProfile = new AppUserProfile()
            {
                FirstName = userProfileDTO.FirstName ?? profileExistInDb.FirstName,
                LastName = userProfileDTO.LastName ?? profileExistInDb.LastName,
                Avatar = userProfileDTO.Avatar ?? profileExistInDb.Avatar
            };

            var result = await _userProfileRepo.UpdateUserProfileAsync(appUserProfile);
            return new UserProfileDTO()
            {
                Id = result?.Id.ToString() ?? "",
                FirstName = result?.FirstName,
                LastName = result?.LastName,
                Avatar = result?.Avatar
            };
        }
    }
}