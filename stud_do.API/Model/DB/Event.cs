using System.Text.Json.Serialization;

namespace stud_do.API.Model
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime? NotificationTime { get; set; }
        public int ScheduleId { get; set; }
        [JsonIgnore]
        public Schedule Schedule { get; set; }
    }
}