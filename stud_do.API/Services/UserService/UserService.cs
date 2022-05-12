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
            var output = new UserOutput() { Email = user.Email, Login = user.Login };
            return new ServiceResponse<UserOutput>() { Data = output };
        }

        public async Task<ServiceResponse<UserOutput>> UpdateUser(UserInput user)
        {
            var userDb = await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();
            userDb.Email = user.Email;
            userDb.Login = user.Login;
            return await GetUser();
        }
    }
}