using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires JWT token
    public class ProfileController : ControllerBase
    {
        private readonly UserRepository userService;

        public ProfileController(UserRepository userRepo)
        {
            userService = userRepo;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ProfileUserDto>> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await userService.GetByUserIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new ProfileUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Mobile = user.Mobile,
                Roles = user.Roles
            });
        }

        [HttpPut("me")]
        public async Task<ActionResult<ProfileUserDto>> UpdateMyProfile([FromBody] UpdateProfileUserDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await userService.GetByUserIdAsync(userId);
            if (user == null)
                return NotFound();

            //check if mobile is used by another user
            var result = await userService.GetByMobileAsync(dto.Mobile);
            if (result != null && result.UserId != user.UserId)
            {
                return Conflict(new { message = "Mobile number is already in use by another user." });
            }

            user.Name = dto.Name;
            user.Email = dto.Email;

            //REVIEW: needs atomicity/transactional processing here, as multiple collections are updated
            if (user.Mobile != dto.Mobile)
            {
                await userService.UpdateMobileReferences(user, dto.Mobile);
            }     

            await userService.UpdateUserProfileAsync(userId, user);

            return Ok(new ProfileUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Mobile = user.Mobile,
                Roles = user.Roles
            });
        }
    }
}
