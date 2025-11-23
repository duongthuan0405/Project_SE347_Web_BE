using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs;
using se347_be.Work.DTOs.Authen;
using se347_be.Work.DTOs.User;

namespace se347_be.Work.Services.Interfaces
{
    public interface IAppAuthenticationService
    {
        public Task<string> SignUpAsync(SignUpRequestDTO signUpRequestDTO);
        public Task<UserResponseDTO?> SignInAsync(SignInRequestDTO signInRequestDTO);
        public Task<string> VerifyAccountAsync(string email, string otp);
        Task ResendOTPAsync(ResendOTPRequestDTO resendOTPDTO);
    }
}