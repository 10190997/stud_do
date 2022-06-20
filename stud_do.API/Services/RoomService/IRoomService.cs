namespace stud_do.API.Services.RoomService
{
    public interface IRoomService
    {
        Task<ServiceResponse<List<RoomOutput>>> GetRooms();

        Task<ServiceResponse<RoomOutput>> CreateRoom(string name);

        Task<ServiceResponse<RoomOutput>> GetRoom(int roomId);

        Task<ServiceResponse<RoomOutput>> UpdateRoom(int roomId, string newName);

        Task<ServiceResponse<List<RoomOutput>>> DeleteRoom(int roomId);

        Task<ServiceResponse<List<RoomOutput>>> SearchRooms(string searchText);

        Task<ServiceResponse<List<string>>> GetRoomSearchSuggestions(string searchText);

        Task<ServiceResponse<RoomOutput>> AddMember(int roomId, int userId);

        Task<ServiceResponse<RoomOutput>> RemoveMember(int roomId, int userId);

        Task<ServiceResponse<RoomOutput>> AddModerator(int roomId, int userId);

        Task<ServiceResponse<RoomOutput>> RemoveModerator(int roomId, int userId);
    }
}