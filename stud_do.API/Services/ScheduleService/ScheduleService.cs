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
                var result = new ServiceResponse<List<ScheduleOutput>>();
                result.Success = false;
                result.Message = ex.Message;
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
                var result = new ServiceResponse<List<ScheduleOutput>>();
                result.Success = false;
                result.Message = ex.Message;
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
                { Message = "schedule not found", Success = false };
            }
            //if (IsCreator(scheduleId))
            //{
            //    var allUs = await _context.UsersSchedules.Where(s => s.ScheduleId == scheduleId).ToListAsync();
            //    if (allUs.Any())
            //    {
            //        _context.UsersSchedules.RemoveRange(allUs);
            //    }
            //}
            _context.Schedules.Remove(schedule);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var result = new ServiceResponse<List<ScheduleOutput>>();
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            return await GetSchedulesAsync();
        }

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
                response.Message = "Schedule does not exist or is unavailable.";
            }

            response.Data = schedule;
            return response;
        }

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
                        result.Add(new ScheduleOutput 
                        { 
                            Created = schedule.Created,
                            CreatorId = schedule.CreatorId,
                            Id = schedule.Id,
                            Events = await _context.Events.Where(e => e.ScheduleId == schedule.Id).ToListAsync(),
                            Name = schedule.Name,
                            Color = scheduleUser.Color, 
                            Visibility = scheduleUser.Visibility 
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
                response.Message = "No schedules found.";
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
                result.Message = "No rights.";
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

        public async Task<ServiceResponse<List<ScheduleOutput>>> AddScheduleAsync(int scheduleId)
        {
            await _context.UsersSchedules.AddAsync(new UsersSchedules() { ScheduleId = scheduleId, UserId = GetUserId() });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var result = new ServiceResponse<List<ScheduleOutput>>();
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return await GetSchedulesAsync();
        }

        public async Task<ServiceResponse<ScheduleOutput>> EditScheduleAsync(ScheduleOutput schedule)
        {
            var userSchedule = await _context.UsersSchedules.Where(us => us.ScheduleId == schedule.Id && us.UserId == GetUserId()).FirstOrDefaultAsync();
            userSchedule.Color = schedule.Color;
            userSchedule.Visibility = schedule.Visibility;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var result = new ServiceResponse<ScheduleOutput>();
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            return await GetScheduleAsync(schedule.Id);
        }
    }
}