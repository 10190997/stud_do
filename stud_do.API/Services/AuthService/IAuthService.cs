namespace stud_do.API.Services.AuthService
{
    public interface IAuthService
    {
        /// <summary>
        /// Решситрация
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="password">Пароль</param>
        Task<ServiceResponse<int>> Register(User user, string password);

        /// <summary>
        /// Вход в систему
        /// </summary>
        /// <param name="email">Адрес электронной почты</param>
        /// <param name="password">Пароль</param>
        Task<ServiceResponse<string>> Login(string email, string password);

        /// <summary>
        /// Смена пароля
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="newPassword">Новый пароль</param>
        Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword);
    }
}