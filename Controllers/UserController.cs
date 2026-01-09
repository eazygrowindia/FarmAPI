using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using FarmAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userService;
        private readonly PasswordHasher _passwordHasher;

        public UserController(UserRepository userService, PasswordHasher passwordHasher)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<User>>?> Get()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("GetUserByUserId")]
        [SwaggerOperation(
            //Summary = "Get User by userId",
            Description = "Returns full details of a user for the given userId.",
            OperationId = "GetUserByUserId"
        )]
        [SwaggerResponse(200, "User found", typeof(User))]
        [SwaggerResponse(404, "User not found")]
        public async Task<ActionResult<User>> GetUser(string userId)
        {
            var existingUser = await _userService.GetByUserIdAsync(userId);

            if (existingUser is null)
            {
                return NotFound();
            }

            return Ok(existingUser);
        }

        [HttpPost("CreateUser")]
        [SwaggerOperation(
            //Summary = "Get User by userId",
            Description = "Creates an user in the system",
            OperationId = "CreateUser"
        )]
        [SwaggerResponse(201, "Created", typeof(User))]
        [SwaggerResponse(400, "Bad Request")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto newUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);  // 400

            var existingUser = await _userService.GetByMobileAsync(newUserDto.Mobile);
            if (existingUser != null) return Conflict("User already exists");

            var newUser = await _userService.CreateUserAsync(newUserDto.Name, newUserDto.Mobile, newUserDto.Email, newUserDto.Roles);

            var (hash, salt) = _passwordHasher.HashPassword(newUserDto.Password);
            newUser.PasswordHash = hash;
            newUser.PasswordSalt = salt;

            await _userService.UpdatePasswordAsync(newUser.UserId, hash, salt);

            //TODO - Password fields should not be sent back with the user data, make json changes to hide it from sending in response
            return CreatedAtAction(nameof(GetUser), new { idUserId = newUser.UserId }, newUser);
            //return Ok(new { message = "User Created", userId = newUser.UserId, mobile = newUser.Mobile });
        }

        [HttpPut("UpdateUserByUserId/{userId}")]
        public async Task<IActionResult> Update(string userId, UpdateUserDto updatedUserDto)
        {
            var existingUser = await _userService.GetByUserIdAsync(userId);

            if (existingUser is null)
            {
                return NotFound();
            }
            
            if (existingUser.Mobile != updatedUserDto.Mobile)
            {
                //check if mobile is used by another user
                var isNewMobileNumberAlreadyInUse = await _userService.IsMobileAlreadyInUse(existingUser.UserId, updatedUserDto.Mobile);
                if (isNewMobileNumberAlreadyInUse)
                {
                    return Conflict(new { message = "Mobile number is already in use by another user." });
                }
            }

            existingUser.Email = updatedUserDto.Email;
            existingUser.Name = updatedUserDto.Name;

            await _userService.UpdateUserRolesAsync(updatedUserDto, existingUser);

            //REVIEW: needs atomicity/transactional processing here, as multiple collections are updated
            if (existingUser.Mobile != updatedUserDto.Mobile)
            {
                await _userService.UpdateMobileReferences(existingUser, updatedUserDto.Mobile);
            }

            var updateResult = await _userService.UpdateAsync(userId, existingUser);

            return Ok(existingUser);
        }

        public record UserSystemStatusUpdateRequest(string userId, string systemStatus);
        [HttpPut("UpdateUserSystemStatus")]
        public async Task<IActionResult> UpdateSystemStatus([FromBody] UserSystemStatusUpdateRequest request)
        {
            var existingUser = await _userService.GetByUserIdAsync(request.userId);

            if (existingUser is null)
            {
                return NotFound();
            }

            var systemStatus = SystemStatusHelper.GetStatus(request.systemStatus).ToString();

            await _userService.UpdateSystemStatusAsync(request.userId, systemStatus);

            return NoContent();
        }

        public record PasswordUpdateRequest(string userId, string Password);

        [HttpPut("UpdateUserPasswordByUserId")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateRequest request)
        {
            var existingUser = await _userService.GetByUserIdAsync(request.userId);

            if (existingUser is null)
            {
                return NotFound();
            }

            try
            {
                var (hash, salt) = _passwordHasher.HashPassword(request.Password);
                existingUser.PasswordHash = hash;
                existingUser.PasswordSalt = salt;

                await _userService.UpdatePasswordAsync(existingUser.UserId, hash, salt);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent() ;
        }

        [HttpDelete("RemoveUserByUserId/{userId}")]
        public async Task<IActionResult> Delete(string userId)
        {
            var existingUser = await _userService.GetByUserIdAsync(userId);

            if (existingUser is null)
            {
                return NotFound();
            }

            await _userService.RemoveAsync(userId);

            return NoContent();
        }
    }
}