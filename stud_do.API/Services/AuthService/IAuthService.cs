namespace stud_do.API.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Register(User user, string password);

        Task<ServiceResponse<string>> Login(string email, string password);

        Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword);
    }
}