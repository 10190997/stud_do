using stud_do.API.Model.DTOs.User;
using System.Security.Claims;

namespace stud_do.API.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Получить всех пользователей системы
        /// </summary>
        public async Task<ServiceResponse<List<UserOutput>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            var response = new ServiceResponse<List<UserOutput>>();
            var result = new List<UserOutput>();
            if (users == null)
            {
                response.Success = false;
                response.Message = "Пользователи не найдены.";
                response.Data = result;
                return response;
            }
            foreach (var user in users)
            {
                result.Add(new UserOutput()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Login = user.Login
                });
            }
            response.Data = result;
            return response;
        }

        /// <summary>
        /// Получить текущего пользователя
        /// </summary>
        public async Task<ServiceResponse<UserOutput>> GetUser()
        {
            var user = await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();
            var output = new UserOutput() { Id = user.Id, Email = user.Email, Login = user.Login };
            return new ServiceResponse<UserOutput>() { Data = output };
        }

        /// <summary>
        /// Редактировать текущего пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        public async Task<ServiceResponse<UserOutput>> UpdateUser(UserInput user)
        {
            var userDb = await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();
            if (userDb == null)
            {
                return new ServiceResponse<UserOutput>
                {
                    Success = false,
                    Message = "Неизвестная ошибка."
                };
            }
            if (user.Email != null)
            {
                if (!AuthService.AuthService.IsValidEmail(user.Email))
                {
                    return new ServiceResponse<UserOutput>
                    {
                        Success = false,
                        Message = "Строка не является Email."
                    };
                }
                userDb.Email = user.Email;
            }
            if (user.Login != null)
            {
                if (!AuthService.AuthService.CheckLogin(user.Login, out string msg))
                {
                    return new ServiceResponse<UserOutput>
                    {
                        Success = false,
                        Message = msg
                    };
                }
                userDb.Login = user.Login;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            return await GetUser();
        }

        /// <summary>
        /// Удалить текущего пользователя
        /// </summary>
        public async Task<bool> DeleteUser()
        {
            var user = await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();
            if (user == null) return false;
            _context.Users.Remove(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Поиск по пользователям
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        public async Task<ServiceResponse<List<UserOutput>>> SearchUsers(string searchText)
        {
            var users = await FindUserBySearchText(searchText);
            if (users.Count == 0)
            {
                return new ServiceResponse<List<UserOutput>>
                {
                    Success = false,
                    Message = "Пользователи не найдены."
                };
            }

            return new ServiceResponse<List<UserOutput>>
            {
                Data = users
            };
        }

        /// <summary>
        /// Поисковые подсказки
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        public async Task<ServiceResponse<List<string>>> GetUserSearchSuggestions(string searchText)
        {
            var users = await FindUserBySearchText(searchText);

            var results = new List<string>();

            foreach (var user in users)
            {
                if (user.Login.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(user.Login);
                }
            }

            if (results.Count == 0)
            {
                return new ServiceResponse<List<string>>
                {
                    Success = false,
                    Message = "Пользователи не найдены."
                };
            }

            return new ServiceResponse<List<string>>
            {
                Data = results
            };
        }

        #region private

        /// <summary>
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// Поиск пользователя по login
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        private async Task<List<UserOutput>> FindUserBySearchText(string searchText)
        {
            var serviceResponse = await GetAllUsers();
            var users = serviceResponse.Data;
            var result = new List<UserOutput>();
            if (users == null)
            {
                return result;
            }
            return users.Where(u => u.Login.ToLower().Contains(searchText.ToLower())).ToList();
        }

        #endregion private
    }
}