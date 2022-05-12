namespace stud_do.API.Services.EventService
{
    public interface IEventService
    {
        Task<ServiceResponse<EventOutput>> GetEventAsync(int eventId);

        Task<ServiceResponse<List<EventOutput>>> GetEventsAsync(int scheduleId);

        Task<ServiceResponse<List<EventOutput>>> DeleteEventAsync(int eventId);

        Task<ServiceResponse<List<EventOutput>>> AddEventAsync(EventInput ev);

        Task<ServiceResponse<EventOutput>> UpdateEventAsync(Event ev);
    }
}