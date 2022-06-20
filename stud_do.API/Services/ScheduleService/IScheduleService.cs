namespace stud_do.API.Services.ScheduleService
{
    public interface IScheduleService
    {
        Task<ServiceResponse<List<ScheduleOutput>>> GetSchedules();

        Task<ServiceResponse<ScheduleOutput>> CreateSchedule(ScheduleInput schedule);

        Task<ServiceResponse<ScheduleOutput>> GetSchedule(int scheduleId);

        Task<ServiceResponse<ScheduleOutput>> UpdateScheduleName(int scheduleId, string scheduleName);

        Task<ServiceResponse<ScheduleOutput>> UpdateScheduleColor(int scheduleId, string color);

        Task<ServiceResponse<List<ScheduleOutput>>> DeleteSchedule(int scheduleId);

        Task<ServiceResponse<ScheduleOutput>> ShowSchedule(int scheduleId);

        Task<ServiceResponse<ScheduleOutput>> HideSchedule(int scheduleId);

        Task<ServiceResponse<ScheduleOutput>> AddUserToSchedule(int scheduleId, int userId);

        Task<ServiceResponse<ScheduleOutput>> RemoveUserFromSchedule(int scheduleId, int userId);
    }
}