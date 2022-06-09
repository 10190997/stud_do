using System.ComponentModel.DataAnnotations;

namespace stud_do.API.Model
{
    public class UserLogin
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}