using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs;
using se347_be.Work.PasswordHelper;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class AuthenticationService : IAppAuthenticationService
    {
        IUserRepository _userRepo;
        IUserProfileRepository _profileRepo;
        public AuthenticationService(IUserRepository userRepository, IUserProfileRepository profileRepository)
        {
            _userRepo = userRepository;
            _profileRepo = profileRepository;
        }
        public async Task<string?> SignInAsync(SignInRequestDTO signInRequestDTO)
        {
            AppUser? user = await _userRepo.GetUserWithGmailAndPasswordAsync(signInRequestDTO.Email, signInRequestDTO.Password);
            return user != null ? user.Id.ToString() : null;
        }

        public async Task<string> SignUpAsync(SignUpRequestDTO signUpRequestDTO)
        {
            if (signUpRequestDTO.Password.Length <= 5)
            {
                throw new InvalidDataException("Password length must have at least 6 characters");
            }

            AppUser user = new AppUser()
            {
                Email = signUpRequestDTO.Email,
                PasswordHash = PasswordHashHelper.HashPassword(signUpRequestDTO.Password)
            };

            AppUserProfile profile = new AppUserProfile()
            {
                LastName = signUpRequestDTO.LastName,
                FirstName = signUpRequestDTO.FirstName
            };

            try
            {
                string id = await _userRepo.AddNewUserAsync(user);
                profile.Id = Guid.Parse(id);
                await _profileRepo.AddNewUserProfileAsync(profile);
                return id;
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception)
            {
                await _userRepo.RemoveByIdAsync(user.Id);
                throw;
            }
        }
    }
}