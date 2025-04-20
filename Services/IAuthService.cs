using HMS.Models;

namespace HMS.Services
{
    public interface IAuthService
    {
        User Authenticate(int id, string password, Role role); // Authenticate user by ID, password, and role
    }
}