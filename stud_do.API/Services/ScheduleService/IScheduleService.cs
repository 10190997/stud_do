namespace stud_do.API.Services.ScheduleService
{
    public interface IScheduleService
    {
        Task<ServiceResponse<ScheduleOutput>> GetScheduleAsync(int scheduleId);

        Task<ServiceResponse<List<ScheduleOutput>>> GetSchedulesAsync();

        Task<ServiceResponse<List<ScheduleOutput>>> DeleteSchedule(int scheduleId);

        /// <summary>
        /// Добавить СУЩЕСТВУЮЩЕЕ расписание к себе
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        Task<ServiceResponse<List<ScheduleOutput>>> AddScheduleAsync(int scheduleId);

        /// <summary>
        /// СОЗДАТЬ расписание с нуля
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        Task<ServiceResponse<List<ScheduleOutput>>> CreateScheduleAsync(ScheduleInput schedule);

        /// <summary>
        /// Изменение САМОГО расписания его создателем
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        Task<ServiceResponse<ScheduleOutput>> UpdateScheduleAsync(string scheduleName, int scheduleId);

        /// <summary>
        /// Изменение ВНЕШНЕГО ВИДА расписания его пользователем
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="color"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        Task<ServiceResponse<ScheduleOutput>> EditScheduleAsync(ScheduleOutput schedule);
    }
}