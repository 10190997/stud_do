namespace stud_do.API.Model
{
    public class ScheduleOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public int? CreatorId { get; set; }
        public string Color { get; set; } = "#555555";
        public bool Visibility { get; set; } = true;
        public List<Event>? Events { get; set; }
        public List<User>? Users { get; set; }
    }
}