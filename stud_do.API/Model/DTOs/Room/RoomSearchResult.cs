namespace stud_do.API.Model
{
    public class RoomSearchResult
    {
        public List<RoomOutput> Rooms { get; set; } = new List<RoomOutput>();
        public int Pages { get; set; }
        public int CurrentPage { get; set; }
    }
}