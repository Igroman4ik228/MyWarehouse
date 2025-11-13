using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;

namespace MyWarehouse.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string login, string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class AuthService(AppDbContext context) : IAuthService
    {
        private readonly AppDbContext _context = context;

        async Task<User?> IAuthService.AuthenticateAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.CURS_Users
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null || !VerifyPassword(password, user.Password))
                return null;

            return user;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}