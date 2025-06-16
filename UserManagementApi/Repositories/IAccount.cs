using static UserManagementApi.Responses.CustomResponses;
using System.Security.Claims;
using UserManagementApi.DTOs;

namespace UserManagementApi.Repositories
{
    public interface IAccount
    {
        public Task<RegisterResponse> CreateUserAsync(CreateUserDto model, ClaimsPrincipal currentUser);
        public Task<LoginResponse> LoginAsync(LoginDTO model);
    }
}
