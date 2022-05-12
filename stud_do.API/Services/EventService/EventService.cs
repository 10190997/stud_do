using System.Security.Claims;

namespace stud_do.API.Services.EventService
{
    public class EventService : IEventService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<EventOutput>>> AddEventAsync(EventInput ev)
        {
            var result = new ServiceResponse<List<EventOutput>>();
            if (!IsCreator(ev.ScheduleId))
            {
                result.Success = false;
                result.Message = "No rights";
                return result;
            }
            Event newEvent = new()
            {
                ScheduleId = ev.ScheduleId,
                Start = ev.Start,
                End = ev.End,
                Name = ev.Name,
                NotificationTime = ev.NotificationTime
            };
            await _context.Events.AddAsync(newEvent);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return await GetEventsAsync(ev.ScheduleId);
        }

        public async Task<ServiceResponse<List<EventOutput>>> DeleteEventAsync(int eventId)
        {
            var result = new ServiceResponse<List<EventOutput>>();
            var dbEvent = await _context.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            if (!IsCreator(dbEvent.ScheduleId))
            {
                result.Success = false;
                result.Message = "No rights";
                return result;
            }
            _context.Events.Remove(dbEvent);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return await GetEventsAsync(dbEvent.ScheduleId);
        }

        public async Task<ServiceResponse<EventOutput>> GetEventAsync(int eventId)
        {
            var result = new ServiceResponse<EventOutput>();
            var ev = await _context.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            EventOutput output = new()
            {
                Name = ev.Name,
                Start = ev.Start,
                End = ev.End,
                NotificationTime = ev.NotificationTime,
                ScheduleId = ev.ScheduleId
            };
            result.Data = output;
            return result;
        }

        // TODO: rework
        private bool IsCreator(int scheduleId)
        {
            var schedule = _context.Schedules.Where(s => s.CreatorId == GetUserId()
                                                        && s.Id == scheduleId).FirstOrDefault();
            return schedule != null;
        }

        public async Task<ServiceResponse<List<EventOutput>>> GetEventsAsync(int scheduleId)
        {
            var result = new ServiceResponse<List<EventOutput>>();
            var events = await _context.Events.Where(e => e.ScheduleId == scheduleId).ToListAsync();
            var outputs = new List<EventOutput>();
            foreach (var e in events)
            {
                EventOutput output = new()
                {
                    Name = e.Name,
                    Start = e.Start,
                    End = e.End,
                    NotificationTime = e.NotificationTime,
                    ScheduleId = e.ScheduleId
                };
                outputs.Add(output);
            }
            result.Data = outputs;
            return result;
        }

        public async Task<ServiceResponse<EventOutput>> UpdateEventAsync(Event ev)
        {
            var result = new ServiceResponse<EventOutput>();
            if (!IsCreator(ev.ScheduleId))
            {
                result.Success = false;
                result.Message = "No rights";
                return result;
            }
            var dbEvent = await _context.Events.Where(e => e.Id == ev.Id).FirstOrDefaultAsync();
            dbEvent.Start = ev.Start;
            dbEvent.End = ev.End;
            dbEvent.Name = ev.Name;
            dbEvent.ScheduleId = ev.ScheduleId;
            dbEvent.NotificationTime = ev.NotificationTime;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            EventOutput output = new()
            {
                Name = ev.Name,
                Start = ev.Start,
                End = ev.End,
                NotificationTime = ev.NotificationTime,
                ScheduleId = ev.ScheduleId
            };
            result.Data = output;
            return result;
        }
    }
}