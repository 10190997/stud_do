namespace stud_do.API.Services.EventService
{
    public interface IEventService
    {
        /// <summary>
        /// Получить событие
        /// </summary>
        /// <param name="eventId">Id события</param>
        Task<ServiceResponse<EventOutput>> GetEventAsync(int eventId);

        /// <summary>
        /// Получить события в расписании
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        Task<ServiceResponse<List<EventOutput>>> GetEventsAsync(int scheduleId);

        /// <summary>
        /// Удалить событие
        /// </summary>
        /// <param name="eventId">Id события</param>
        Task<ServiceResponse<List<EventOutput>>> DeleteEventAsync(int eventId);

        /// <summary>
        /// Создать событие и добавить его к расписанию
        /// </summary>
        /// <param name="ev">Событие</param>
        /// <param name="scheduleId">Id расписания</param>
        Task<ServiceResponse<List<EventOutput>>> AddEventAsync(EventInput ev, int scheduleId);

        /// <summary>
        /// Изменить событие
        /// </summary>
        /// <param name="ev">Событие</param>
        /// <param name="eventId">Id события</param>
        Task<ServiceResponse<EventOutput>> UpdateEventAsync(EventInput ev, int eventId);
    }
}