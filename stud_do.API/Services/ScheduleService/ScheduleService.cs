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
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<ScheduleOutput>>> CreateScheduleAsync(ScheduleInput scheduleDTO)
        {
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
                var result = new ServiceResponse<List<ScheduleOutput>>
                {
                    Success = false,
                    Message = ex.Message
                };
                return result;
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
                var result = new ServiceResponse<List<ScheduleOutput>>
                {
                    Success = false,
                    Message = ex.Message
                };
                return result;
            }
            return await GetSchedulesAsync();
        }

        public async Task<ServiceResponse<List<ScheduleOutput>>> DeleteSchedule(int scheduleId)
        {
            var us = await _context.UsersSchedules.Where(u => u.UserId == GetUserId() && u.ScheduleId == scheduleId).FirstOrDefaultAsync();
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null)
            {
                return new ServiceResponse<List<ScheduleOutput>>()
                { Message = "Расписание не найдено", Success = false };
            }
            if (!IsCreator(scheduleId))
            {
                return new ServiceResponse<List<ScheduleOutput>>()
                { Message = "Нет прав на удаление расписания", Success = false };
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
                    Message = ex.Message
                };
                return result;
            }

            return await GetSchedulesAsync();
        }

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

        public async Task<ServiceResponse<ScheduleOutput>> GetScheduleAsync(int scheduleId)
        {
            var schedulesUsers = await GetSchedulesAsync();
            var response = new ServiceResponse<ScheduleOutput>();
            var schedule = schedulesUsers.Data.Find(us => us.Id == scheduleId);

            if (schedule == null)
            {
                response.Success = false;
                response.Message = "Расписание не найдено.";
            }

            response.Data = schedule;
            return response;
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
                            Created = schedule.Created,
                            CreatorId = schedule.CreatorId,
                            Id = schedule.Id,
                            Events = await _context.Events.Where(e => e.ScheduleId == schedule.Id).ToListAsync(),
                            Name = schedule.Name,
                            Color = scheduleUser.Color ?? "#555555",
                            Visibility = scheduleUser.Visibility,
                            Users = users
                        });
                    }
                }
            }
            return result;
        }

        public async Task<ServiceResponse<List<ScheduleOutput>>> GetSchedulesAsync()
        {
            var uSchedules = await GetSchedulesForUser();
            var response = new ServiceResponse<List<ScheduleOutput>>();
            if (uSchedules.Count < 1)
            {
                response.Success = false;
                response.Message = "Расписания не найдены.";
            }
            response.Data = uSchedules;
            return response;
        }

        public async Task<ServiceResponse<ScheduleOutput>> UpdateScheduleAsync(string scheduleName, int scheduleId)
        {
            var schedulesUsers = await _context.UsersSchedules.Where(us => us.UserId == GetUserId()).ToListAsync();
            var scheduleUser = schedulesUsers.FirstOrDefault(us => us.ScheduleId == scheduleId);
            var result = new ServiceResponse<ScheduleOutput>();
            if (!IsCreator(scheduleId))
            {
                result.Success = false;
                result.Message = "Нет прав.";
                return result;
            }
            var scheduleDB = await _context.Schedules.Where(s => s.Id == scheduleId && s.CreatorId == GetUserId()).FirstOrDefaultAsync();
            scheduleDB.Name = scheduleName;
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

            return await GetScheduleAsync(scheduleId);
        }

        /// <summary>
        /// Изменить внешний вид расписания
        /// </summary>
        /// <param name="schedule"></param>
        public async Task<ServiceResponse<ScheduleOutput>> EditScheduleAsync(int scheduleId, string color, bool visibility)
        {
            var userSchedule = await _context.UsersSchedules.Where(us => us.ScheduleId == scheduleId && us.UserId == GetUserId()).FirstOrDefaultAsync();
            userSchedule.Color = color;
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
                    Message = ex.Message
                };
                return result;
            }

            return await GetScheduleAsync(scheduleId);
        }

        /// <summary>
        /// Выдать доступ к расписанию
        /// </summary>
        public async Task<ServiceResponse<ScheduleOutput>> AddUserToScheduleAsync(int scheduleId, int userId)
        {
            var response = new ServiceResponse<ScheduleOutput>();
            var schedule = await _context.Schedules.Where(r => r.Id == scheduleId).FirstOrDefaultAsync();
            if (schedule == null)
            {
                response.Success = false;
                response.Message = "Расписание не найдено.";
                return response;
            }
            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                response.Success = false;
                response.Message = "Пользователь не найден.";
                return response;
            }
            var us = GetMemberUserSchedule(scheduleId, userId);
            if (us.UserId != 0)
            {
                response.Success = false;
                response.Message = "Пользователь уже имеет доступ к расписанию";
                return response;
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
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return await GetScheduleAsync(scheduleId);
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
    }
}