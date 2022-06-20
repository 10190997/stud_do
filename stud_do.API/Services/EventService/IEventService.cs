namespace stud_do.API.Services.EventService
{
    public interface IEventService
    {
        Task<ServiceResponse<List<EventOutput>>> GetEvents(int scheduleId);

        Task<ServiceResponse<EventOutput>> CreateEvent(EventInput ev);

        Task<ServiceResponse<EventOutput>> GetEvent(int eventId);

        Task<ServiceResponse<EventOutput>> UpdateEvent(int eventId, EventInput ev);

        Task<ServiceResponse<List<EventOutput>>> DeleteEvent(int eventId);
    }
}