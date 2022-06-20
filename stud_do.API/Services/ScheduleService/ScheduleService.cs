using System.Globalization;
using System.Security.Claims;

namespace stud_do.API.Services.ScheduleService
{
    public class ScheduleService : IScheduleService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ScheduleService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Получить расписания
        /// </summary>
        public async Task<ServiceResponse<List<ScheduleOutput>>> GetSchedules()
        {
            var uSchedules = await GetSchedulesForUser();
            if (uSchedules.Count < 1)
            {
                return new ServiceResponse<List<ScheduleOutput>>
                {
                    Success = false,
                    Message = "Расписания не найдены."
                };
            }
            return new ServiceResponse<List<ScheduleOutput>>
            {
                Data = uSchedules
            };
        }

        /// <summary>
        /// Создать расписание
        /// </summary>
        /// <param name="schedule">Расписание</param>
        public async Task<ServiceResponse<ScheduleOutput>> CreateSchedule(ScheduleInput scheduleDTO)
        {
            if (!uint.TryParse(scheduleDTO.Color, NumberStyles.HexNumber, null, out uint _))
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Неверно указан цвет. Формат должен быть hex."
                };
            }
            Schedule schedule = new()
            {
                Name = scheduleDTO.Name,
                Created = DateTime.Now,
                CreatorId = GetUserId()
            };
            await _context.Schedules.AddAsync(schedule);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            UsersSchedules us = new()
            {
                ScheduleId = schedule.Id,
                UserId = GetUserId(),
                Color = scheduleDTO.Color,
                Visibility = true
            };

            await _context.UsersSchedules.AddAsync(us);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            return await GetSchedule(schedule.Id);
        }

        /// <summary>
        /// Получить расписание
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        public async Task<ServiceResponse<ScheduleOutput>> GetSchedule(int scheduleId)
        {
            var schedulesUsers = await GetSchedules();
            if (schedulesUsers.Data == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Расписания не найдены."
                };
            }

            var schedule = schedulesUsers.Data.Find(us => us.Id == scheduleId);
            if (schedule == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Расписание не найдено."
                };
            }
            return new ServiceResponse<ScheduleOutput>
            {
                Data = schedule
            };
        }

        /// <summary>
        /// Редактировать расписание
        /// </summary>
        /// <param name="scheduleName">Новое название</param>
        /// <param name="scheduleId">Id расписания</param>
        public async Task<ServiceResponse<ScheduleOutput>> UpdateScheduleName(int scheduleId, string scheduleName)
        {
            if (!IsCreator(scheduleId))
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Нет прав."
                };
            }

            var schedulesUsers = await _context.UsersSchedules.Where(us => us.UserId == GetUserId()).ToListAsync();
            var scheduleUser = schedulesUsers.FirstOrDefault(us => us.ScheduleId == scheduleId);
            var scheduleDB = await _context.Schedules.Where(s => s.Id == scheduleId && s.CreatorId == GetUserId()).FirstOrDefaultAsync();
            if (scheduleDB == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Расписание не найдено."
                };
            }

            scheduleDB.Name = scheduleName;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetSchedule(scheduleId);
        }

        /// <summary>
        /// Изменить цвет расписания
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="color"></param>
        public async Task<ServiceResponse<ScheduleOutput>> UpdateScheduleColor(int scheduleId, string color)
        {
            var userSchedule = await _context.UsersSchedules.Where(us => us.ScheduleId == scheduleId && us.UserId == GetUserId()).FirstOrDefaultAsync();
            if (userSchedule == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Расписание не найдено."
                };
            }

            userSchedule.Color = color;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetSchedule(scheduleId);
        }

        /// <summary>
        /// Удалить расписание
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        public async Task<ServiceResponse<List<ScheduleOutput>>> DeleteSchedule(int scheduleId)
        {
            var us = await _context.UsersSchedules.Where(u => u.UserId == GetUserId() && u.ScheduleId == scheduleId).FirstOrDefaultAsync();
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null)
            {
                return new ServiceResponse<List<ScheduleOutput>>()
                {
                    Message = "Расписание не найдено",
                    Success = false
                };
            }
            if (!IsCreator(scheduleId))
            {
                return new ServiceResponse<List<ScheduleOutput>>()
                {
                    Message = "Нет прав на удаление расписания",
                    Success = false
                };
            }
            var allUs = await _context.UsersSchedules.Where(s => s.ScheduleId == scheduleId).ToListAsync();
            if (allUs.Any())
            {
                _context.UsersSchedules.RemoveRange(allUs);
            }
            _context.Schedules.Remove(schedule);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var result = new ServiceResponse<List<ScheduleOutput>>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
                return result;
            }

            return await GetSchedules();
        }

        public async Task<ServiceResponse<ScheduleOutput>> ShowSchedule(int scheduleId)
        {
            return await ToggleVisibility(scheduleId, true);
        }

        public async Task<ServiceResponse<ScheduleOutput>> HideSchedule(int scheduleId)
        {
            return await ToggleVisibility(scheduleId, false);
        }

        /// <summary>
        /// Выдать доступ к расписанию
        /// </summary>
        public async Task<ServiceResponse<ScheduleOutput>> AddUserToSchedule(int scheduleId, int userId)
        {
            var schedule = await _context.Schedules.Where(r => r.Id == scheduleId).FirstOrDefaultAsync();
            if (schedule == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Расписание не найдено."
                };
            }
            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Пользователь не найден."
                };
            }
            var us = GetMemberUserSchedule(scheduleId, userId);
            if (us.UserId != 0)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Пользователь уже имеет доступ к расписанию"
                };
            }
            var userSchedule = new UsersSchedules
            {
                ScheduleId = scheduleId,
                UserId = userId
            };

            await _context.UsersSchedules.AddAsync(userSchedule);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetSchedule(scheduleId);
        }

        public async Task<ServiceResponse<ScheduleOutput>> RemoveUserFromSchedule(int scheduleId, int userId)
        {
            var s = await _context.Schedules.Where(sc => sc.Id == scheduleId).FirstOrDefaultAsync();
            if (s == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Расписание не найдено."
                };
            }
            var us = await _context.UsersSchedules.Where(u => u.UserId == userId && u.ScheduleId == scheduleId).FirstOrDefaultAsync();

            if (us == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Пользователь не имеет доступа к расписанию"
                };
            }

            if (us.UserId != GetUserId())
            {
                if (!IsCreator(scheduleId))
                {
                    return new ServiceResponse<ScheduleOutput>
                    {
                        Success = false,
                        Message = "Нет прав."
                    };
                }
            }
            if (s.CreatorId == userId)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = "Создатель расписания не может от него отписаться."
                };
            }
            _context.UsersSchedules.Remove(us);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetSchedule(scheduleId);
        }

        #region private

        private async Task<ServiceResponse<ScheduleOutput>> ToggleVisibility(int scheduleId, bool visibility)
        {
            var userSchedule = await _context.UsersSchedules.Where(us => us.ScheduleId == scheduleId && us.UserId == GetUserId()).FirstOrDefaultAsync();
            if (userSchedule == null)
            {
                return new ServiceResponse<ScheduleOutput>
                {
                    Success = true,
                    Message = "Расписание не найдено."
                };
            }

            userSchedule.Visibility = visibility;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var result = new ServiceResponse<ScheduleOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
                return result;
            }

            return await GetSchedule(scheduleId);
        }

        /// <summary>
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// Проверка создателя расписания
        /// </summary>
        /// <param name="scheduleId"></param>
        private bool IsCreator(int scheduleId)
        {
            var schedule = _context.Schedules.Where(s => s.CreatorId == GetUserId()
                                                        && s.Id == scheduleId).FirstOrDefault();
            return schedule != null;
        }

        /// <summary>
        /// Получить расписания пользователя
        /// </summary>
        private async Task<List<ScheduleOutput>> GetSchedulesForUser()
        {
            var schedulesUsers = await _context.UsersSchedules.Where(us => us.UserId == GetUserId()).ToListAsync();
            var result = new List<ScheduleOutput>();
            if (schedulesUsers.Count != 0)
            {
                var schedules = await _context.Schedules.ToListAsync();
                foreach (var schedule in schedules)
                {
                    var scheduleUser = schedulesUsers.FirstOrDefault(us => us.ScheduleId == schedule.Id);
                    if (scheduleUser != null)
                    {
                        var schedulesForUsers = await _context.UsersSchedules.Where(us => us.ScheduleId == schedule.Id).ToListAsync();
                        var users = new List<User>();
                        foreach (var user in schedulesForUsers)
                        {
                            var u = _context.Users.Where(u => u.Id == user.UserId).FirstOrDefault();
                            if (u != null)
                            {
                                users.Add(u);
                            }
                        }
                        result.Add(new ScheduleOutput
                        {
                            Created = schedule.Created.ToString("dd.MM.yyyy HH:mm"),
                            CreatorId = schedule.CreatorId,
                            Id = schedule.Id,
                            Events = await _context.Events.Where(e => e.ScheduleId == schedule.Id).ToListAsync(),
                            Name = schedule.Name,
                            Color = scheduleUser.Color ?? "ffffff",
                            Visibility = scheduleUser.Visibility,
                            Users = users
                        });
                    }
                }
            }
            return result;
        }

        private UsersSchedules GetMemberUserSchedule(int scheduleId, int userId)
        {
            var users = _context.UsersSchedules.Where(x => x.UserId == userId).ToList();
            var us = users.Where(x => x.ScheduleId == scheduleId).FirstOrDefault();
            if (us == null)
            {
                return new UsersSchedules();
            }
            return us;
        }

        #endregion private
    }
}