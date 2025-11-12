using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.UserProfile;
using se347_be.Work.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        IUserProfileService _userProfileService;
        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
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
    }
}