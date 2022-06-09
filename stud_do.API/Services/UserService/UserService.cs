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
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

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

        public async Task<ServiceResponse<UserOutput>> GetUser()
        {
            var user = await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();
            var output = new UserOutput() { Id = user.Id, Email = user.Email, Login = user.Login };
            return new ServiceResponse<UserOutput>() { Data = output };
        }

        public async Task<ServiceResponse<UserOutput>> UpdateUser(UserInput user)
        {
            if (!AuthService.AuthService.IsValidEmail(user.Email))
            {
                return new ServiceResponse<UserOutput>
                {
                    Success = false,
                    Message = "Строка не являетс Email"
                };
            }
            var userDb = await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();
            userDb.Email = user.Email;
            userDb.Login = user.Login;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserOutput>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            return await GetUser();
        }

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

        public async Task<ServiceResponse<List<UserOutput>>> SearchUsersAsync(string searchText)
        {
            var users = await FindUserBySearchTextAsync(searchText);

            var response = new ServiceResponse<List<UserOutput>>
            {
                Data = users
            };

            return response;
        }

        public async Task<ServiceResponse<List<string>>> GetUserSearchSuggestionsAsync(string searchText)
        {
            var users = await FindUserBySearchTextAsync(searchText);

            var results = new List<string>();

            foreach (var user in users)
            {
                if (user.Login.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(user.Login);
                }
            }

            return new ServiceResponse<List<string>>
            {
                Data = results
            };
        }

        /// <summary>
        /// Поиск пользователя по email
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        private async Task<List<UserOutput>> FindUserBySearchTextAsync(string searchText)
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
    }
}