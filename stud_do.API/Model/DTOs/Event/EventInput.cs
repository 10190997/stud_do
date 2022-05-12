namespace stud_do.API.Model
{
    public class EventInput
    {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime? NotificationTime { get; set; }
        public int ScheduleId { get; set; }
    }
}
