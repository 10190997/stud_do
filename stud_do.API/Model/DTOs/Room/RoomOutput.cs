namespace stud_do.API.Model
{
    public class RoomOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } = "user";
        public List<UserRoomOutput>? Users { get; set; }
        public List<PostOutput>? Posts { get; set; }
    }
}