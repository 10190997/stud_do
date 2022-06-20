namespace stud_do.API.Model
{
    public class UserRoomOutput
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
    }
}