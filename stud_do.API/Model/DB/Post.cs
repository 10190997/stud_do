using System.Text.Json.Serialization;

namespace stud_do.API.Model
{
    public class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int RoomId { get; set; }
        [JsonIgnore]
        public Room Room { get; set; }

        //[JsonIgnore]
        public List<Attachment>? Attachments { get; set; }
    }
}