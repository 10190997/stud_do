namespace stud_do.API.Services.ScheduleService
{
    public interface IScheduleService
    {
        /// <summary>
        /// Получить расписание 
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        Task<ServiceResponse<ScheduleOutput>> GetScheduleAsync(int scheduleId);

        /// <summary>
        /// Получить расписания
        /// </summary>
        Task<ServiceResponse<List<ScheduleOutput>>> GetSchedulesAsync();

        /// <summary>
        /// Удалить расписание
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        Task<ServiceResponse<List<ScheduleOutput>>> DeleteSchedule(int scheduleId);

        /// <summary>
        /// Создать расписание
        /// </summary>
        /// <param name="schedule">Расписание</param>
        Task<ServiceResponse<List<ScheduleOutput>>> CreateScheduleAsync(ScheduleInput schedule);

        /// <summary>
        /// Редактировать расписание
        /// </summary>
        /// <param name="scheduleName">Новое название</param>
        /// <param name="scheduleId">Id расписания</param>
        Task<ServiceResponse<ScheduleOutput>> UpdateScheduleAsync(string scheduleName, int scheduleId);

        /// <summary>
        /// Изменение ВНЕШНЕГО ВИДА расписания его пользователем
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        /// <param name="color">Новый цвет</param>
        /// <param name="visibility">Отображение</param>
        Task<ServiceResponse<ScheduleOutput>> EditScheduleAsync(int scheduleId, string color, bool visibility);

        /// <summary>
        /// Добавить пользователю расписание
        /// </summary>
        /// <param name="scheduleId">Id расписания</param>
        /// <param name="userId">Id пользователя</param>
        Task<ServiceResponse<ScheduleOutput>> AddUserToScheduleAsync(int scheduleId, int userId);
    }
}