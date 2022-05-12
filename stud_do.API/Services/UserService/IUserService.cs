using stud_do.API.Model.DTOs.User;

namespace stud_do.API.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<UserOutput>> GetUser();

        Task<bool> DeleteUser();

        Task<ServiceResponse<UserOutput>> UpdateUser(UserInput user);
    }
}