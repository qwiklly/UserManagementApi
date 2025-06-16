using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementApi.Data;
using UserManagementApi.Models;
using static UserManagementApi.Responses.CustomResponses;
using UserManagementApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace UserManagementApi.Repositories
{
    public class Account : IAccount
    {

        private readonly ApplicationDbContext _appDbContext;
        private readonly IConfiguration _config;

        public Account(ApplicationDbContext appDbContext, IConfiguration config)
        {
            _appDbContext = appDbContext;
            _config = config;
        }

        public async Task<LoginResponse> LoginAsync(LoginDTO model)
        {
            try
            {
                var findUser = await GetUserAsync(model.Login);
                if (findUser == null || string.IsNullOrEmpty(findUser.Password) || !BCrypt.Net.BCrypt.Verify(model.Password, findUser.Password))
                    return new LoginResponse(false, "Invalid email or password.");

                string jwtToken = GenerateToken(findUser);
                return new LoginResponse(true, "Login successfully", jwtToken);
            }
            catch (Exception)
            {
                return new LoginResponse(false, "Error occurred while signing in");
            }
        }

        public async Task<RegisterResponse> CreateUserAsync(CreateUserDto model, ClaimsPrincipal currentUser)
        {
            try
            {
                var findUser = await GetUserAsync(model.Login);
                if (findUser != null) return new RegisterResponse(false, "User already exists");


                bool isInAdminRole = false;

                if (model.Admin == true) 
                {
                    isInAdminRole = true;
                }
             
                if ((isInAdminRole == true) && !currentUser.IsInRole("Admin"))
                {
                    return new RegisterResponse(false, "Only admins can create Admin account.");
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Login = model.Login,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Name = model.Name,
                    Gender = model.Gender,
                    Birthday = model.Birthday,
                    Admin = isInAdminRole,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = currentUser.FindFirstValue("Login") ?? "System",
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedBy = currentUser.FindFirstValue("Login") ?? "System",
                    RevokedOn = null,
                    RevokedBy = null
                };

                _appDbContext.Users.Add(newUser);
                await _appDbContext.SaveChangesAsync();

                return new RegisterResponse(true, "User registered successfully");
            }
            catch (Exception)
            {
                return new RegisterResponse(false, "Error occurred while signing up");
            }
        }

        public async Task<User?> GetUserAsync(string login)
            => await _appDbContext.Users.FirstOrDefaultAsync(e => e.Login ==login);

        // Generates a JWT token based on the user's details.
        private string GenerateToken(User user)
        {
            try
            {
                var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing in configuration.");
                var jwtIssuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing in configuration.");
                var jwtAudience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing in configuration.");

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var userClaims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                    new Claim("Login", user.Login)
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: userClaims,
                    expires: DateTime.UtcNow.AddDays(2),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate token", ex);
            }
        }
       
    }
}
