using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.UserProfile;
using se347_be.Work.Services.Interfaces;
using se347_be.Work.URLFileHelper;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        IUserProfileService _userProfileService;
        IUserService _userService;
        public UserProfileController(IUserProfileService userProfileService, IUserService userService)
        {
            _userProfileService = userProfileService;
            _userService = userService;
        }

        private string GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            return userIdClaim;
        }

        [HttpPut]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult<UserProfileResponseDTO>> UpdateProfile([FromForm] UpdateUserProfileRequestDTO updateDto)
        {
            var userId = GetCurrentUserId().ToString();

            try
            {
                var result = await _userProfileService.UpdateProfileAsync(userId, updateDto, updateDto.ImageFile);
                if (result == null) 
                { 
                    return StatusCode(StatusCodes.Status404NotFound, new { Message = "User not found!" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<UserProfileResponseDTO>> GetCurrentUserProfile()
        {
            try
            {
                var userId = GetCurrentUserId().ToString();
                var result = await _userProfileService.GetProfileByIdAsync(userId);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { Message = "User not found!" });
                }
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("full")]
        [Authorize]
        public async Task<ActionResult<UserProfileFullDTO>> GetCurrentUserProfileFull()
        {
            try
            {
                var userId = GetCurrentUserId().ToString();
    
                var profile = await _userProfileService.GetProfileByIdAsync(userId);
                var user = await _userService.GetUserByIdAsync(userId);


                if (profile == null || user == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { Message = "User not found!" });
                }
                return Ok(new UserProfileFullDTO { User = user, UserProfile = profile});
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
    }
}