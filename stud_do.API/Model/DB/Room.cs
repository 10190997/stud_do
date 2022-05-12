using System.Text.Json.Serialization;

namespace stud_do.API.Model
{
    /// <summary>
    /// Комната
    /// </summary>
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual List<UserRoom>? UsersRooms { get; set; }
    }
}