using System.Text.Json.Serialization;

namespace stud_do.API.Model
{
    public class UserRoom
    {
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public int RoomId { get; set; }
        [JsonIgnore]
        public Room Room { get; set; }
        public int RoleId { get; set; }
        [JsonIgnore]
        public Role Role { get; set; }
    }
}