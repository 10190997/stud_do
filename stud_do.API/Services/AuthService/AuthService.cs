using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace stud_do.API.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string>();
            if (!IsValidEmail(email))
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Строка не является Email."
                };
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));
            if (user == null)
            {
                response.Success = false;
                response.Message = "Пользователь с таким Email не найден.";
                return response;
            }
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Неверный пароль.";
                return response;
            }

            response.Data = CreateToken(user);
            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            if (await UserExists(user.Email))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = "Пользователь с таким Email уже существует."
                };
            }

            if (!IsValidEmail(user.Email))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = "Строка не является Email."
                };
            }

            if (!CheckPassword(password, out string msg))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = msg
                };
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = ex.Message
                };
            }

            return new ServiceResponse<int>
            {
                Data = user.Id,
                Message = "Регистрация прошла успешно."
            };
        }

        public async Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (!CheckPassword(newPassword, out string msg))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = msg
                };
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }

            return new ServiceResponse<bool>
            {
                Data = true,
                Message = "Пароль был успешно изменен."
            };
        }

        /// <summary>
        /// Проверка на существование пользователя по Email
        /// </summary>
        /// <param name="email">Электронная почта</param>
        private async Task<bool> UserExists(string email)
        {
            if (await _context.Users.AnyAsync(user => user.Email.ToLower()
                 .Equals(email.ToLower())))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Хеширование пароля
        /// </summary>
        /// <param name="password">Пароль</param>
        /// <param name="passwordHash">Хэшированный пароль</param>
        /// <param name="passwordSalt">Соль</param>
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac
                .ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Сверка паролей по их хэшу
        /// </summary>
        /// <param name="password">Пароль</param>
        /// <param name="passwordHash">Хэшированный пароль</param>
        /// <param name="passwordSalt">Соль</param>
        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash =
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        /// <summary>
        /// Генерация токена
        /// </summary>
        /// <param name="user">Пользователь</param>
        private string CreateToken(User user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        /// <summary>
        /// Проверка надежности пароля
        /// </summary>
        /// <param name="password">Пароль</param>
        /// <param name="message">Сообщение</param>
        private static bool CheckPassword(string password, out string message)
        {
            if (password.Length < 6)
            {
                message = "Пароль должен иметь длину не менее 6 символов.";
                return false;
            }

            if (password.Length > 20)
            {
                message = "Пароль должен иметь длину не более 20 символов.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                message = "Пароль должен содержать как минимум один прописной символ.";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                message = "Пароль должен содержать как минимум один строчный символ.";
                return false;
            }

            string specialChars = @"%!@#$%^&*()?/>.<,:;'\|}]{[_~`+=-" + "\"";
            char[] specialCharsArr = specialChars.ToCharArray();
            foreach (char ch in specialCharsArr)
            {
                if (password.Contains(ch))
                {
                    message = "";
                    return true;
                }
            }
            message = "Пароль должен содержать как минимум один специальный символ.";
            return false;
        }

        /// <summary>
        /// Проверка валидности адреса электронной почты
        /// </summary>
        /// <param name="email">Адрес электронной почты</param>
        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }

}