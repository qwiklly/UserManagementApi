using static UserManagementApi.Responses.CustomResponses;
using System.Security.Claims;
using UserManagementApi.DTOs;

namespace UserManagementApi.Repositories
{
    public interface IUserRepo
    {
        Task<BaseResponse> UpdateProfileAsync(string login, UpdateUserDto dto, ClaimsPrincipal currentUser);
        Task<BaseResponse> ChangePasswordAsync(string login, UpdatePasswordDto dto, ClaimsPrincipal currentUser);
        Task<BaseResponse> ChangeLoginAsync(string login, UpdateLoginDto dto, ClaimsPrincipal currentUser);
        Task<GenericResponse<List<UserListDto>>> GetActiveUsersAsync();
        Task<GenericResponse<UserInfoDto>> GetByLoginAdminAsync(string login);
        Task<BaseResponse> AuthenticateForSelfAsync(string login, string password, ClaimsPrincipal currentUser);
        Task<GenericResponse<List<UserInfoDto>>> GetUsersOlderThanAsync(int age);
        Task<BaseResponse> DeleteUserAsync(string login, bool hardDelete, ClaimsPrincipal currentUser);
        Task<BaseResponse> RestoreUserAsync(string login, ClaimsPrincipal currentUser);
    }
}
