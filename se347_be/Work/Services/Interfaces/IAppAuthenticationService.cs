using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.DTOs;

namespace se347_be.Work.Services.Interfaces
{
    public interface IAppAuthenticationService
    {
        public Task<string> SignUpAsync(SignUpRequestDTO signUpRequestDTO);
        public Task<string?> SignInAsync(SignInRequestDTO signInRequestDTO);
        public Task<string> VerifyAccountAsync(string email, string otp);
        Task ResendOTPAsync(ResendOTPRequestDTO resendOTPDTO);
    }
}