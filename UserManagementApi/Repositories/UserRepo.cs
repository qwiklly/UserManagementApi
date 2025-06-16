using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserManagementApi.Data;
using UserManagementApi.DTOs;
using UserManagementApi.Models;
using static UserManagementApi.Responses.CustomResponses;

namespace UserManagementApi.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _appDbContext;
        public UserRepo(ApplicationDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<GenericResponse<UserInfoDto>> GetByLoginAdminAsync(string login)
        {
            try
            {
                var u = await GetUserAsync(login);
                if (u == null)
                    return new GenericResponse<UserInfoDto>(false, "User not found.");

                var dto = new UserInfoDto
                {
                    Login = u.Login,
                    Name = u.Name,
                    Gender = u.Gender,
                    Birthday = u.Birthday,
                    IsActive = u.RevokedOn == null
                };
                return new GenericResponse<UserInfoDto>(true, "OK", dto);
            }
            catch
            {
                return new GenericResponse<UserInfoDto>(false, "Failed to fetch user.");
            }
        }

        public async Task<GenericResponse<List<UserListDto>>> GetActiveUsersAsync()
        {
            try
            {
                var list = await _appDbContext.Users
                    .Where(u => u.RevokedOn == null)
                    .OrderBy(u => u.CreatedOn)
                    .Select(u => new UserListDto
                    {
                        Id = u.Id,
                        Login = u.Login,
                        CreatedOn = u.CreatedOn
                    })
                    .ToListAsync();

                return new GenericResponse<List<UserListDto>>(true, "OK", list);
            }
            catch
            {
                return new GenericResponse<List<UserListDto>>(false, "Failed to get active users.");
            }
        }

        public async Task<GenericResponse<List<UserInfoDto>>> GetUsersOlderThanAsync(int age)
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddYears(-age);
                var list = await _appDbContext.Users
                    .Where(u => u.RevokedOn == null && u.Birthday != null && u.Birthday <= cutoff)
                    .Select(u => new UserInfoDto
                    {
                        Login = u.Login,
                        Name = u.Name,
                        Gender = u.Gender,
                        Birthday = u.Birthday,
                        IsActive = true
                    }).ToListAsync();

                return new GenericResponse<List<UserInfoDto>>(true, "OK", list);
            }
            catch
            {
                return new GenericResponse<List<UserInfoDto>>(false, "Failed to query users.");
            }
        }

        public async Task<BaseResponse> UpdateProfileAsync(string login, UpdateUserDto dto, ClaimsPrincipal currentUser)
        {
            try
            {
                var u = await GetUserAsync(login);
                if (u == null || u.RevokedOn != null)
                    return new BaseResponse(false, "User not found or revoked.");

                var caller = currentUser.FindFirstValue("Login");
                var isAdmin = currentUser.IsInRole("Admin");
                if (!isAdmin && caller != login)
                    return new BaseResponse(false, "Forbidden.");

                u.Name = dto.Name;
                u.Gender = dto.Gender;
                u.Birthday = dto.Birthday;
                u.ModifiedOn = DateTime.UtcNow;
                u.ModifiedBy = caller!;

                await _appDbContext.SaveChangesAsync();
                return new BaseResponse(true, "Profile updated.");
            }
            catch
            {
                return new BaseResponse(false, "Failed to update profile.");
            }
        }

        public async Task<BaseResponse> ChangePasswordAsync(string login, UpdatePasswordDto dto, ClaimsPrincipal currentUser)
        {
            try
            {
                var u = await GetUserAsync(login);
                if (u == null || u.RevokedOn != null)
                    return new BaseResponse(false, "User not found or revoked.");

                var caller = currentUser.FindFirstValue("Login");
                var isAdmin = currentUser.IsInRole("Admin");
                if (!isAdmin && caller != login)
                    return new BaseResponse(false, "Forbidden.");

                if (!isAdmin && !BCrypt.Net.BCrypt.Verify(dto.OldPassword, u.Password))
                    return new BaseResponse(false, "Old password is incorrect.");

                u.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                u.ModifiedOn = DateTime.UtcNow;
                u.ModifiedBy = caller!;

                await _appDbContext.SaveChangesAsync();
                return new BaseResponse(true, "Password changed.");
            }
            catch
            {
                return new BaseResponse(false, "Failed to change password.");
            }
        }

        public async Task<BaseResponse> ChangeLoginAsync(string login, UpdateLoginDto dto, ClaimsPrincipal currentUser)
        {
            try
            {
                var u = await GetUserAsync(login);
                if (u == null || u.RevokedOn != null)
                    return new BaseResponse(false, "User not found or revoked.");

                var caller = currentUser.FindFirstValue("Login");
                var isAdmin = currentUser.IsInRole("Admin");
                if (!isAdmin && caller != login)
                    return new BaseResponse(false, "Forbidden.");

                if (await _appDbContext.Users.AnyAsync(x => x.Login == dto.NewLogin))
                    return new BaseResponse(false, "Login already in use.");

                u.Login = dto.NewLogin;
                u.ModifiedOn = DateTime.UtcNow;
                u.ModifiedBy = caller!;

                await _appDbContext.SaveChangesAsync();
                return new BaseResponse(true, "Login changed.");
            }
            catch
            {
                return new BaseResponse(false, "Failed to change login.");
            }
        }

        public async Task<BaseResponse> DeleteUserAsync(string login, bool hardDelete, ClaimsPrincipal currentUser)
        {
            try
            {
                if (!currentUser.IsInRole("Admin"))
                    return new BaseResponse(false, "Only admins can delete.");

                var u = await GetUserAsync(login);
                if (u == null)
                    return new BaseResponse(false, "User not found.");

                if (hardDelete)
                    _appDbContext.Users.Remove(u);
                else
                {
                    u.RevokedOn = DateTime.UtcNow;
                    u.RevokedBy = currentUser.FindFirstValue("Login");
                }

                await _appDbContext.SaveChangesAsync();
                return new BaseResponse(true, "User deleted.");
            }
            catch
            {
                return new BaseResponse(false, "Failed to delete user.");
            }
        }

        public async Task<BaseResponse> RestoreUserAsync(string login, ClaimsPrincipal currentUser)
        {
            try
            {
                if (!currentUser.IsInRole("Admin"))
                    return new BaseResponse(false, "Only admins can restore.");

                var u = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login && u.RevokedOn != null);
                if (u == null)
                    return new BaseResponse(false, "User not found or not revoked.");

                u.RevokedOn = null;
                u.RevokedBy = null;
                u.ModifiedOn = DateTime.UtcNow;
                u.ModifiedBy = currentUser.FindFirstValue("Login");

                await _appDbContext.SaveChangesAsync();
                return new BaseResponse(true, "User restored.");
            }
            catch
            {
                return new BaseResponse(false, "Failed to restore user.");
            }
        }

        public async Task<BaseResponse> AuthenticateForSelfAsync(string login, string password, ClaimsPrincipal currentUser)
        {
            try
            {
                var u = await _appDbContext.Users
                    .FirstOrDefaultAsync(u => u.Login == login && u.RevokedOn == null);

                if (u == null)
                    return new GenericResponse<UserInfoDto>(false, "Пользователь не найден или не активен.");

                var callerId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (callerId != u.Id.ToString())
                    return new GenericResponse<UserInfoDto>(false, "Доступ запрещён.");

                if (!BCrypt.Net.BCrypt.Verify(password, u.Password))
                    return new GenericResponse<UserInfoDto>(false, "Неверный пароль.");

                var dto = new UserInfoDto
                {
                    Login = u.Login,
                    Name = u.Name,
                    Gender = u.Gender,
                    Birthday = u.Birthday,
                    IsActive = true
                };
                return new GenericResponse<UserInfoDto>(true, "OK", dto);
            }
            catch
            {
                return new GenericResponse<UserInfoDto>(false, "Ошибка при аутентификации.");
            }
        }

        public async Task<User?> GetUserAsync(string login)
        => await _appDbContext.Users.FirstOrDefaultAsync(e => e.Login == login);
    }
}
