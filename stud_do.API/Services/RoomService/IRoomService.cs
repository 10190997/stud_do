namespace stud_do.API.Services.RoomService
{
    public interface IRoomService
    {
        Task<ServiceResponse<RoomOutput>> GetRoomAsync(int roomId);

        Task<ServiceResponse<List<RoomOutput>>> GetRoomsAsync();

        Task<ServiceResponse<RoomSearchResult>> SearchRoomsAsync(string searchText, int page);

        Task<ServiceResponse<List<string>>> GetRoomSearchSuggestionsAsync(string searchText);

        Task<ServiceResponse<List<RoomOutput>>> DeleteRoom(int roomId);

        Task<ServiceResponse<List<RoomOutput>>> AddRoomAsync(string name);

        Task<ServiceResponse<List<RoomOutput>>> UpdateRoomAsync(Room room);
    }
}