using System.Globalization;
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
        /// Получить события в расписании
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        public async Task<ServiceResponse<List<EventOutput>>> GetEvents(int scheduleId)
        {
            var events = await _context.Events.Where(e => e.ScheduleId == scheduleId).ToListAsync();
            if (events.Count == 0)
            {
                return new ServiceResponse<List<EventOutput>>
                {
                    Success = false,
                    Message = "События не найдены."
                };
            }
            var schUser = await _context.UsersSchedules.Where(e => e.ScheduleId == scheduleId && e.UserId == GetUserId()).FirstOrDefaultAsync();
            if (schUser == null)
            {
                return new ServiceResponse<List<EventOutput>>
                {
                    Success = false,
                    Message = "Нет доступа к расписанию."
                };
            }
            var outputs = new List<EventOutput>();
            foreach (var e in events)
            {
                EventOutput output = new()
                {
                    Id = e.Id,
                    Name = e.Name,
                    Start = e.Start.ToString("dd.MM.yyyy HH:mm"),
                    End = e.End.ToString("dd.MM.yyyy HH:mm"),
                    NotificationTime = e.NotificationTime == null ? null : ((DateTime)e.NotificationTime).ToString("dd.MM.yyyy HH:mm"),
                    ScheduleId = e.ScheduleId
                };
                outputs.Add(output);
            }
            return new ServiceResponse<List<EventOutput>>
            {
                Data = outputs
            };
        }

        /// <summary>
        /// Создать событие и добавить его к расписанию
        /// </summary>
        /// <param name="ev">Событие</param>
        /// <param name="scheduleId">Id расписания</param>
        public async Task<ServiceResponse<EventOutput>> CreateEvent(EventInput ev)
        {
            if (!Parse(ev.Start, ev.End, ev.NotificationTime, out Dates d))
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = "Не удалось определить формат даты."
                };
            }
            var e = new EventDateInput
            {
                Name = ev.Name,
                ScheduleId = ev.ScheduleId,
                Start = d.Start,
                End = d.End,
                NotificationTime = d.NotificationTime
            };
            return await CreateEvent(e);
        }

        /// <summary>
        /// Получить событие
        /// </summary>
        /// <param name="eventId">Id события</param>
        public async Task<ServiceResponse<EventOutput>> GetEvent(int eventId)
        {
            var ev = await _context.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            if (ev == null)
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = "Событие не найдено."
                };
            }
            var schUser = await _context.UsersSchedules.Where(e => e.ScheduleId == ev.ScheduleId && e.UserId == GetUserId()).FirstOrDefaultAsync();
            if (schUser == null)
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = "Нет доступа к расписанию."
                };
            }
            EventOutput output = new()
            {
                Id = eventId,
                Name = ev.Name,
                Start = ev.Start.ToString("dd.MM.yyyy HH:mm"),
                End = ev.End.ToString("dd.MM.yyyy HH:mm"),
                NotificationTime = ev.NotificationTime == null ? "" : ((DateTime)ev.NotificationTime).ToString("dd.MM.yyyy HH:mm"),
                ScheduleId = ev.ScheduleId
            };
            return new ServiceResponse<EventOutput>
            {
                Data = output
            };
        }

        /// <summary>
        /// Изменить событие
        /// </summary>
        /// <param name="ev">Событие</param>
        /// <param name="eventId">Id события</param>
        public async Task<ServiceResponse<EventOutput>> UpdateEvent(int eventId, EventInput ev)
        {
            if (!Parse(ev.Start, ev.End, ev.NotificationTime, out Dates d))
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = "Не удалось определить формат даты."
                };
            }
            var e = new EventDateInput
            {
                Name = ev.Name,
                ScheduleId = ev.ScheduleId,
                Start = d.Start,
                End = d.End,
                NotificationTime = d.NotificationTime
            };
            return await UpdateEvent(eventId, e);
        }

        /// <summary>
        /// Удалить событие
        /// </summary>
        /// <param name="eventId">Id события</param>
        public async Task<ServiceResponse<List<EventOutput>>> DeleteEvent(int eventId)
        {
            var dbEvent = await _context.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            if (dbEvent == null)
            {
                return new ServiceResponse<List<EventOutput>>
                {
                    Success = false,
                    Message = "Событие не найдено."
                };
            }
            if (!IsCreator(dbEvent.ScheduleId))
            {
                return new ServiceResponse<List<EventOutput>>
                {
                    Success = false,
                    Message = "Нет прав на изменение расписания."
                };
            }
            _context.Events.Remove(dbEvent);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<EventOutput>>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            return await GetEvents(dbEvent.ScheduleId);
        }

        #region private

        private async Task<ServiceResponse<EventOutput>> UpdateEvent(int eventId, EventDateInput ev)
        {
            var dbEvent = await _context.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            if (dbEvent == null)
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = "Событие не найдено."
                };
            }
            if (!IsCreator(dbEvent.ScheduleId))
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = "Нет прав на изменение расписания."
                };
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
                    return new ServiceResponse<EventOutput>
                    {
                        Success = false,
                        Message = msg
                    };
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
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            EventOutput output = new()
            {
                Id = eventId,
                Name = ev.Name,
                Start = ev.Start.ToString("dd.MM.yyyy HH:mm"),
                End = ev.End.ToString("dd.MM.yyyy HH:mm"),
                NotificationTime = ev.NotificationTime == null ? "" : ((DateTime)ev.NotificationTime).ToString("dd.MM.yyyy HH:mm"),
                ScheduleId = dbEvent.ScheduleId
            };
            return new ServiceResponse<EventOutput>
            {
                Data = output
            };
        }

        private class Dates
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public DateTime? NotificationTime { get; set; }
        }

        private static bool Parse(string start, string end, string? notif, out Dates dates)
        {
            var result = new Dates();
            if (!DateTime.TryParseExact(start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime s))
            {
                dates = result;
                return false;
            }
            if (!DateTime.TryParseExact(end, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out DateTime e))
            {
                dates = result;
                return false;
            }
            if (string.IsNullOrEmpty(notif) || string.IsNullOrWhiteSpace(notif))
            {
                result.NotificationTime = null;
            }
            else
            {
                if (!DateTime.TryParseExact(notif, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime n))
                {
                    dates = result;
                    return false;
                }
                else
                {
                    result.NotificationTime = n;
                }
            }
            result.Start = s;
            result.End = e;
            dates = result;
            return true;
        }

        private async Task<ServiceResponse<EventOutput>> CreateEvent(EventDateInput ev)
        {
            if (!IsCreator(ev.ScheduleId))
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = "Нет прав на изменение расписания."
                };
            }
            if (!CheckEventTime(ev, ev.ScheduleId, out string msg))
            {
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = msg
                };
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
                return new ServiceResponse<EventOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            return await GetEvent(newEvent.Id);
        }

        /// <summary>
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// Проверка времени события
        /// </summary>
        /// <param name="ev">Событие</param>
        /// <param name="msg">Сообщение</param>
        private bool CheckEventTime(EventDateInput ev, int scheduleId, out string msg)
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

        #endregion private
    }
}