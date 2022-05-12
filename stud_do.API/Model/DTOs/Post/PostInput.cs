namespace stud_do.API.Model
{
    public class PostInput
    {
        public string Text { get; set; }
        public int RoomId { get; set; }
        public List<Attachment>? Attachments { get; set; }
    }
}
