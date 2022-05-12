using System.Text.Json.Serialization;

namespace stud_do.API.Model
{
    /// <summary>
    /// Пользователь
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        [JsonIgnore]
        public virtual List<UserRoom>? UsersRooms { get; set; }

        [JsonIgnore]
        public virtual List<UsersSchedules>? Schedules { get; set; }
    }
}