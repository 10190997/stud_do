namespace stud_do.API.Model
{
    public class EventInput
    {
        public string Name { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string? NotificationTime { get; set; }
        public int ScheduleId { get; set; }
    }
}