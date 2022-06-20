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

            if (!CheckLogin(user.Login, out string msgLogin))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = msgLogin
                };
            }

            if (!CheckPassword(password, out string msgPassword))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = msgPassword
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
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return new ServiceResponse<int>
            {
                Data = user.Id,
                Message = "Регистрация прошла успешно."
            };
        }

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
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
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Пользователь с таким Email не найден."
                };
            }

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Неверный пароль.",
                };
            }

            return new ServiceResponse<string>
            {
                Data = CreateToken(user),
                Message = "Авторизация прошла успешно."
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
                    Message = "Пользователь не найден."
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
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return new ServiceResponse<bool>
            {
                Message = "Пароль был успешно изменен."
            };
        }

        #region private

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

            if (!password.Any(char.IsNumber))
            {
                message = "Пароль должен содержать как минимум одну цифру.";
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

        /// <summary>
        /// Проверка валидности логина
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="msg">Сообщение</param>
        public static bool CheckLogin(string login, out string msg)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrWhiteSpace(login))
            {
                msg = "Заполните поле логин.";
                return false;
            }

            if (login.Length < 6)
            {
                msg = "Минимальная длина логина - 6 символов.";
                return false;
            }

            if (login.Length > 20)
            {
                msg = "Максимальная длина логина - 20 символов.";
                return false;
            }

            msg = "";
            return true;
        }

        #endregion private
    }
}