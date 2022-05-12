using System.Text.Json.Serialization;

namespace stud_do.API.Model
{
    public class UsersSchedules
    {
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public int ScheduleId { get; set; }
        [JsonIgnore]
        public Schedule Schedule { get; set; }
        public string? Color { get; set; } = "#555555";
        public bool Visibility { get; set; } = true;
    }
}