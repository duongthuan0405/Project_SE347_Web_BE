using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs;
using se347_be.Work.JWT;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers.Authentication
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        IAppAuthenticationService _authService;
        JWTHelper _jwtHelper;
        IUserRepository _userRepository;
        public AuthenticationController(IAppAuthenticationService authenticationService, JWTHelper jwtHelper, IUserRepository userRepository)
        {
            _authService = authenticationService;
            _jwtHelper = jwtHelper;
            _userRepository = userRepository;
        }
        
        [HttpPost("sign-up")]
        public async Task<ActionResult<string>> SignUp([FromBody] SignUpRequestDTO signUpRequest)
        {
            try
            {
                string id = await _authService.SignUpAsync(signUpRequest);
                return Ok(id);
            }
            catch (InvalidDataException idEx)
            {
                return BadRequest(new { Message = idEx.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            } 
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<string>> SignIn([FromBody] SignInRequestDTO signInRequestDTO)
        {
            try
            {
                string? userId = await _authService.SignInAsync(signInRequestDTO);
                if (userId == null)
                {
                    return BadRequest(new { Message = "Either Email or Password is incorrect!" });
                }

                string token = _jwtHelper.GenerateToken(userId);
                return Ok(token);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "There was a problem with Sign in Process");
            }

        }

        [HttpPost("verify")]
        public async Task<ActionResult<string>> VerifyAccount([FromBody]VerifyAccountDTO verifyAccountDTO)
        {
            try
            {
                string id = await _authService.VerifyAccountAsync(verifyAccountDTO.Email, verifyAccountDTO.OTP);
                return Ok(id);
            }
            catch (InvalidDataException idEx)
            {
                return BadRequest(new { Message = idEx.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Verification process is unsuccessful!" });
            }
        }

        [HttpPost("resend-otp")]
        public async Task<ActionResult> ResendOTP([FromBody] ResendOTPRequestDTO resendOTPDTO)
        {
            try
            {
                await _authService.ResendOTPAsync(resendOTPDTO);
                return Ok();
            }
            catch (InvalidDataException idEx)
            {
                return BadRequest(new { Message = idEx.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Verification process is unsuccessful!" });
            }

        }

    }

}