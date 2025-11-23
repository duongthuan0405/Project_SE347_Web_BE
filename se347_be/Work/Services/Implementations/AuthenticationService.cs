using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs;
using se347_be.Work.DTOs.Authen;
using se347_be.Work.DTOs.User;
using se347_be.Work.Email;
using se347_be.Work.PasswordHelper;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class AuthenticationService : IAppAuthenticationService
    {
        IUserRepository _userRepo;
        IUserProfileRepository _profileRepo;
        IPendingUserRepository _pendingUserRepo;
        IEmailService _email;
        public AuthenticationService(IUserRepository userRepository, IUserProfileRepository profileRepository, IEmailService email, IPendingUserRepository pendingUserRepo)
        {
            _userRepo = userRepository;
            _profileRepo = profileRepository;
            _email = email;
            _pendingUserRepo = pendingUserRepo;
        }

        public async Task ResendOTPAsync(ResendOTPRequestDTO resendOTPDTO)
        {
            await _email.SendOTPAsync(resendOTPDTO.EmailTo, true);
        }

        public async Task<UserResponseDTO?> SignInAsync(SignInRequestDTO signInRequestDTO)
        {
            AppUser? user = await _userRepo.GetUserWithGmailAndPasswordAsync(signInRequestDTO.Email, signInRequestDTO.Password);
            if (user == null)
            {
                return null;
            }

            return new UserResponseDTO()
            {
                Email = user.Email,
                Id = user.Id.ToString(),
            };
                
        }

        public async Task<string> SignUpAsync(SignUpRequestDTO signUpRequestDTO)
        {
            if (signUpRequestDTO.Password.Length <= 5)
            {
                throw new InvalidDataException("Password length must have at least 6 characters");
            }

            var userWithEmail = await _userRepo.FindUserByEmail(signUpRequestDTO.Email);
            if(userWithEmail != null)
            {
                
                throw new InvalidDataException("This email has been used!");
            }
            
            try
            {
                PendingUser pendingUser = new PendingUser()
                {
                    Email = signUpRequestDTO.Email,
                    PasswordHash = PasswordHashHelper.HashPassword(signUpRequestDTO.Password),
                    LastName = signUpRequestDTO.LastName,
                    FirstName = signUpRequestDTO.FirstName
                };

                string emailUser = await _pendingUserRepo.AddNewPendingUserAsync(pendingUser);
                await _email.SendOTPAsync(emailUser);
                return emailUser;
            }
            catch (Exception)
            {
                await _pendingUserRepo.RemovePendingUserAsync(signUpRequestDTO.Email);
                throw;
            }
           
        }

        public async Task<string> VerifyAccountAsync(string email, string otp)
        {
            Guid id = Guid.Empty;
            try
            {
                var result = await _pendingUserRepo.VerifyAccountAsync(email, otp);
                AppUser user = new AppUser()
                {
                    Email = result.Email,
                    PasswordHash = result.PasswordHash,
                };

                id = await _userRepo.AddNewUserAsync(user);

                AppUserProfile profile = new AppUserProfile()
                {
                    LastName = result.LastName,
                    FirstName = result.FirstName,
                    Id = id
                };

                await _profileRepo.AddNewUserProfileAsync(profile);
                return id.ToString();
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch(Exception)
            {
                await _userRepo.RemoveByIdAsync(id);
                throw;
            }
      
        }
    }
}