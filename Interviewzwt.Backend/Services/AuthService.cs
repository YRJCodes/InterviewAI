using Interviewzwt.Backend.Data;
using Interviewzwt.Backend.DTOs;
using Interviewzwt.Backend.Entities;
using Interviewzwt.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace Interviewzwt.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> Register(RegisterRequest request)
        {
            if (!IsStrongPassword(request.Password))
                throw new ArgumentException("Password must be at least 8 characters and include uppercase, lowercase, number, and symbol.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return null;

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BC.HashPassword(request.Password),
                Credits = 10
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => "@$!%*?&".Contains(ch));
        }

        public async Task<AuthResponse?> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BC.Verify(request.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("email", user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Credits = user.Credits
            };
        }
    }
}
