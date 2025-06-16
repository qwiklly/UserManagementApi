using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserManagementApi.DTOs;
using UserManagementApi.Repositories;

namespace UserManagementApi.Controllers
{
    [Route("api/application")]
    [ApiController]
    public class AccountController(IAccount _accountrepo) : ControllerBase
    {
        [HttpPost("createNewUser")]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user account. Only admins can create admin accounts."
        )]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto model)
        {
            var currentUser = HttpContext.User;

            var result = await _accountrepo.CreateUserAsync(model, currentUser);
            return Ok(result);
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Login a user",
            Description = "Authenticate a user with email and password. Returns JWT token."
        )]
        public async Task<IActionResult> LoginAsync(LoginDTO model)
        {
            var result = await _accountrepo.LoginAsync(model);
            return Ok(result);
        }
    }
}
