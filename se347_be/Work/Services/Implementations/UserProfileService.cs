using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.UserProfile;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;
using se347_be.Work.Storage.Interfaces;
using se347_be.Work.URLFileHelper;

namespace se347_be.Work.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepo;
        private readonly IImageStorage _imageStorage;
        private readonly IURLHelper _urlFileHelper;
    
        public UserProfileService(IUserProfileRepository userProfileRepository, IImageStorage imageStorage, IURLHelper uRLFileHelper)
        {
            _userProfileRepo = userProfileRepository;
            _imageStorage = imageStorage;
            _urlFileHelper = uRLFileHelper;            
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
                Avatar =_urlFileHelper.GetLiveURL(appUserProfile.Avatar ?? "") 
            };

            return userProfileDTO;
        }

        public async Task<UserProfileResponseDTO?> UpdateProfileAsync(string id, UpdateUserProfileRequestDTO updateRequest, IFormFile? avatar)
        {
            try
            {
                string avatarURL = "";
                var currentUser = await _userProfileRepo.GetProfileByIdAsync(Guid.Parse(id));
                if (currentUser == null)
                {
                    return null;
                }

                if (avatar != null)
                {
                    avatarURL = await _imageStorage.SaveAsync(avatar, "avatars");
                    _imageStorage.Delete(currentUser.Avatar ?? "");
                }

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
                    Avatar = _urlFileHelper.GetLiveURL(result?.Avatar ?? "")
                };
            } catch (Exception) {
                throw;
            }
        }
    }
}