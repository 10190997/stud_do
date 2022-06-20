using stud_do.API.Model.DTOs.User;

namespace stud_do.API.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<List<UserOutput>>> GetAllUsers();

        Task<ServiceResponse<UserOutput>> GetUser();

        Task<ServiceResponse<UserOutput>> UpdateUser(UserInput user);

        Task<bool> DeleteUser();

        Task<ServiceResponse<List<UserOutput>>> SearchUsers(string searchText);

        Task<ServiceResponse<List<string>>> GetUserSearchSuggestions(string searchText);
    }
}