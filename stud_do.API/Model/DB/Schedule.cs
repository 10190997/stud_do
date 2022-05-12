using System.Text.Json.Serialization;

namespace stud_do.API.Model
{
    public class Schedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public int? CreatorId { get; set; }
        [JsonIgnore]
        public User? Creator { get; set; }

        [JsonIgnore]
        public virtual List<Event>? Events { get; set; }

        [JsonIgnore]
        public virtual List<UsersSchedules>? Users { get; set; }
    }
}