using Interviewzwt.Backend.DTOs;
using Interviewzwt.Backend.Entities;

namespace Interviewzwt.Backend.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> Register(RegisterRequest request);
        Task<AuthResponse?> Login(LoginRequest request);
        string GenerateJwtToken(User user);
        Task<User?> GetUserById(Guid userId);
    }
}
