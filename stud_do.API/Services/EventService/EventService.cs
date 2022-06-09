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

        /// <summary>
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<EventOutput>>> AddEventAsync(EventInput ev, int scheduleId)
        {
            var result = new ServiceResponse<List<EventOutput>>();
            if (!IsCreator(scheduleId))
            {
                result.Success = false;
                result.Message = "Нет прав на изменение расписания.";
                return result;
            }
            if (!CheckEventTime(ev, scheduleId, out string msg))
            {
                result.Success = false;
                result.Message = msg;
                return result;
            }
            Event newEvent = new()
            {
                ScheduleId = scheduleId,
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
            return await GetEventsAsync(scheduleId);
        }

        public async Task<ServiceResponse<List<EventOutput>>> DeleteEventAsync(int eventId)
        {
            var result = new ServiceResponse<List<EventOutput>>();
            var dbEvent = await _context.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            if (dbEvent == null)
            {
                result.Success = false;
                result.Message = "Событие не найдено.";
                return result;
            }
            if (!IsCreator(dbEvent.ScheduleId))
            {
                result.Success = false;
                result.Message = "Нет прав на изменение расписания.";
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
            if (ev == null)
            {
                result.Success = false;
                result.Message = "Событие не найдено.";
                return result;
            }
            var schUser = await _context.UsersSchedules.Where(e => e.ScheduleId == ev.ScheduleId && e.UserId == GetUserId()).FirstOrDefaultAsync();
            if (schUser == null)
            {
                result.Success = false;
                result.Message = "Нет доступа к расписанию.";
                return result;
            }
            EventOutput output = new()
            {
                Id = eventId,
                Name = ev.Name,
                Start = ev.Start,
                End = ev.End,
                NotificationTime = ev.NotificationTime,
                ScheduleId = ev.ScheduleId
            };
            result.Data = output;
            return result;
        }

        /// <summary>
        /// Проверка прав на изменение расписания
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
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
            if (events.Count == 0)
            {
                result.Success = false;
                result.Message = "События не найдены.";
                return result;
            }
            var schUser = await _context.UsersSchedules.Where(e => e.ScheduleId == scheduleId && e.UserId == GetUserId()).FirstOrDefaultAsync();
            if (schUser == null)
            {
                result.Success = false;
                result.Message = "Нет доступа к расписанию.";
                return result;
            }
            var outputs = new List<EventOutput>();
            foreach (var e in events)
            {
                EventOutput output = new()
                {
                    Id = e.Id,
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

        public async Task<ServiceResponse<EventOutput>> UpdateEventAsync(EventInput ev, int eventId)
        {
            var result = new ServiceResponse<EventOutput>();
            var dbEvent = await _context.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            if (dbEvent == null)
            {
                result.Success = false;
                result.Message = "Событие не найдено.";
                return result;
            }
            if (!IsCreator(dbEvent.ScheduleId))
            {
                result.Success = false;
                result.Message = "Нет прав на изменение расписания.";
                return result;
            }
            if (ev.Start == DateTime.MinValue)
            {
                ev.Start = dbEvent.Start;
            }
            if (ev.End == DateTime.MinValue)
            {
                ev.End = dbEvent.End;
            }
            if (ev.NotificationTime == null)
            {
                ev.NotificationTime = dbEvent.NotificationTime;
            }
            if (string.IsNullOrEmpty(ev.Name))
            {
                ev.Name = dbEvent.Name;
            }
            if (ev.Start != dbEvent.Start || ev.End != dbEvent.End)
            {
                if (!CheckEventTime(ev, dbEvent.ScheduleId, out string msg))
                {
                    result.Success = false;
                    result.Message = msg;
                    return result;
                }
            }

            dbEvent.Start = ev.Start;
            dbEvent.End = ev.End;
            dbEvent.Name = ev.Name;
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
                Id = eventId,
                Name = ev.Name,
                Start = ev.Start,
                End = ev.End,
                NotificationTime = ev.NotificationTime,
                ScheduleId = dbEvent.ScheduleId
            };
            result.Data = output;
            return result;
        }

        /// <summary>
        /// Проверка времени события
        /// </summary>
        /// <param name="ev">Событие</param>
        /// <param name="msg">Сообщение</param>
        private bool CheckEventTime(EventInput ev, int scheduleId, out string msg)
        {
            List<Event> events = _context.Events.Where(s => s.ScheduleId == scheduleId).ToList();
            foreach (var item in events)
            {
                if (item.Start == ev.Start)
                {
                    msg = "Время начала данного события совпадает с временем начала другого события в этом расписании.";
                    return false;
                }
                if (item.End > ev.Start)
                {
                    msg = "Время начала данного события находится в промежутке другого события в этом расписании.";
                    return false;
                }
            }
            msg = "";
            return true;
        }
    }
}