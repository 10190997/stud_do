namespace stud_do.API.Services.RoomService
{
    public interface IRoomService
    {
        /// <summary>
        /// Получить комнату
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        Task<ServiceResponse<RoomOutput>> GetRoomAsync(int roomId);

        /// <summary>
        /// Вывести список комнат, доступных пользователю
        /// </summary>
        Task<ServiceResponse<List<RoomOutput>>> GetRoomsAsync();

        /// <summary>
        /// Получить список комнат по названию
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        Task<ServiceResponse<List<RoomOutput>>> SearchRoomsAsync(string searchText);

        /// <summary>
        /// Получить поисковые подсказки
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        Task<ServiceResponse<List<string>>> GetRoomSearchSuggestionsAsync(string searchText);

        /// <summary>
        /// Удалить комнату
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        Task<ServiceResponse<List<RoomOutput>>> DeleteRoom(int roomId);

        /// <summary>
        /// Создать комнату
        /// </summary>
        /// <param name="name">Название</param>
        Task<ServiceResponse<List<RoomOutput>>> AddRoomAsync(string name);

        /// <summary>
        /// Изменить комнату
        /// </summary>
        /// <param name="room">Комната</param>
        Task<ServiceResponse<RoomOutput>> UpdateRoomAsync(string newName, int roomId);

        /// <summary>
        /// Добавить участника комнаты
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        Task<ServiceResponse<RoomOutput>> AddMemberAsync(int roomId, int userId);

        /// <summary>
        /// Назначить редактора комнаты
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        Task<ServiceResponse<RoomOutput>> AddModeratorAsync(int roomId, int userId);
    }
}