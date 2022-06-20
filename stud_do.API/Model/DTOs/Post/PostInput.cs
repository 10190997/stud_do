namespace stud_do.API.Model
{
    public class PostInput
    {
        public string Text { get; set; }
        public List<string>? Attachments { get; set; }
        public int RoomId { get; set; }
    }
}