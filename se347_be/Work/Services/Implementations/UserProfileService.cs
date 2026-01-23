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

        public async Task<UserProfileResponseDTO?> UpdateProfileAsync(
             string id,
             UpdateUserProfileRequestDTO updateRequest,
             IFormFile? avatar)
        {
            Guid userId = Guid.Parse(id);

            var currentUser = await _userProfileRepo.GetProfileByIdAsync(userId);
            if (currentUser == null)
            {
                return null;
            }

            string? oldAvatar = currentUser.Avatar;
            string? newAvatarPath = null;

        
            if (avatar != null)
            {
                newAvatarPath = await _imageStorage.SaveAsync(avatar, "avatars");
                currentUser.Avatar = newAvatarPath;
            }

         
            if (updateRequest.FirstName != null)
                currentUser.FirstName = updateRequest.FirstName;

            if (updateRequest.LastName != null)
                currentUser.LastName = updateRequest.LastName;

            var updatedUser = await _userProfileRepo.UpdateUserProfileAsync(currentUser);

            if (avatar != null && !string.IsNullOrEmpty(oldAvatar))
            {
                _imageStorage.Delete(oldAvatar);
            }

            return new UserProfileResponseDTO
            {
                Id = updatedUser.Id.ToString(),
                FirstName = updatedUser.FirstName,
                LastName = updatedUser.LastName,
                Avatar = _urlFileHelper.GetLiveURL(updatedUser.Avatar ?? "")
            };
        }

    }
}