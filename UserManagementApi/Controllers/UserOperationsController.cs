using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserManagementApi.DTOs;
using UserManagementApi.Repositories;

namespace UserManagementApi.Controllers
{
    [Route("api/application")]
    [ApiController]
    public class UserOperationsController(IUserRepo _userRepo) : ControllerBase
    {
        [HttpPatch("{login}/profile")]
        [SwaggerOperation(
            Summary = "Update user profile",
            Description = "Updates name, gender, or birthday. Allowed for admin or the active user themselves."
        )]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string login, [FromBody] UpdateUserDto dto)
        {
            var result = await _userRepo.UpdateProfileAsync(login, dto, User);
            return Ok(result);
        }

        [HttpPatch("{login}/password")]
        [SwaggerOperation(
            Summary = "Change password",
            Description = "Changes user's password. Allowed for admin or the active user themselves."
        )]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string login, [FromBody] UpdatePasswordDto dto)
        {
            var result = await _userRepo.ChangePasswordAsync(login, dto, User);
            return Ok(result);
        }

        [HttpPatch("{login}/login")]
        [SwaggerOperation(
            Summary = "Change login",
            Description = "Changes user's login (must be unique). Allowed for admin or the active user themselves."
        )]
        [Authorize]
        public async Task<IActionResult> ChangeLogin(string login, [FromBody] UpdateLoginDto dto)
        {
            var result = await _userRepo.ChangeLoginAsync(login, dto, User);
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get active users",
            Description = "Returns a list of all active users (without RevokedOn). Only for admins."
        )]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActive()
            => Ok(await _userRepo.GetActiveUsersAsync());

        [HttpGet("{login}/info")]
        [SwaggerOperation(
            Summary = "Get user info (admin)",
            Description = "Returns user info by login (name, gender, birthday, status). Only for admins."
        )]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByLoginAdmin(string login)
        {
            var dto = await _userRepo.GetByLoginAdminAsync(login);
            return dto == null ? NotFound() : Ok(dto);
        }

        [HttpPost("{login}/authenticate-self")]
        [SwaggerOperation(
            Summary = "Self-authenticate",
            Description = "Verifies a user's login and password. Only for the user themselves (if active)."
        )]
        [Authorize]
        public async Task<IActionResult> AuthenticateSelf(string login, [FromBody] LoginDTO dto)
        {
            var result = await _userRepo.AuthenticateForSelfAsync(login, dto.Password, User);
            return Ok(result);
        }

        [HttpGet("older-than/{age}")]
        [SwaggerOperation(
            Summary = "Get users older than age",
            Description = "Returns list of users older than the given age. Only for admins."
        )]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOlderThan(int age)
            => Ok(await _userRepo.GetUsersOlderThanAsync(age));

        [HttpDelete("{login}")]
        [SwaggerOperation(
            Summary = "Delete user",
            Description = "Deletes a user by login. Supports soft (default) and hard deletion. Only for admins."
        )]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string login, [FromQuery] bool hard = false)
        {
            var result = await _userRepo.DeleteUserAsync(login, hard, User);
            return Ok(result);
        }

        [HttpPost("{login}/restore")]
        [SwaggerOperation(
            Summary = "Restore user",
            Description = "Restores a soft-deleted user (clears RevokedOn and RevokedBy). Only for admins."
        )]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreUser(string login)
        {
            var result = await _userRepo.RestoreUserAsync(login, User);
            return Ok(result);
        }
    }
}
