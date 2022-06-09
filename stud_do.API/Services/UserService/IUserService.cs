using stud_do.API.Model.DTOs.User;

namespace stud_do.API.Services.UserService
{
    public interface IUserService
    {
        /// <summary>
        /// Получить текущего пользователя
        /// </summary>
        Task<ServiceResponse<UserOutput>> GetUser();

        /// <summary>
        /// Получить всех пользователей системы
        /// </summary>
        Task<ServiceResponse<List<UserOutput>>> GetAllUsers();

        /// <summary>
        /// Удалить текущего пользователя
        /// </summary>
        Task<bool> DeleteUser();

        /// <summary>
        /// Редактировать текущего пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        Task<ServiceResponse<UserOutput>> UpdateUser(UserInput user);

        /// <summary>
        /// Поиск по пользователям
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        Task<ServiceResponse<List<UserOutput>>> SearchUsersAsync(string searchText);

        /// <summary>
        /// Поисковые подсказки
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        Task<ServiceResponse<List<string>>> GetUserSearchSuggestionsAsync(string searchText);
    }
}