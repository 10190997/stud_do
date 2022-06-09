using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Services;
using stud_do.API.Services.ScheduleService;

namespace stud_do.API.Controllers
{
    [EnableCors("_myAllowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ServiceResponse<List<ScheduleOutput>>>> GetSchedules()
        {
            var result = await _scheduleService.GetSchedulesAsync();
            return Ok(result);
        }

        [HttpGet("get/schedule={scheduleId}")]
        public async Task<ActionResult<ScheduleOutput>> GetSchedule(int scheduleId)
        {
            var result = await _scheduleService.GetScheduleAsync(scheduleId);
            return Ok(result);
        }

        [HttpPost("delete/schedule={scheduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ScheduleOutput>>>> DeleteSchedule(int scheduleId)
        {
            var result = await _scheduleService.DeleteSchedule(scheduleId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> CreateSchedule(ScheduleInput schedule)
        {
            var result = await _scheduleService.CreateScheduleAsync(schedule);
            return Ok(result);
        }

        [HttpPost("creator-update/schedule={scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> UpdateSchedule(string scheduleName, int scheduleId)
        {
            var result = await _scheduleService.UpdateScheduleAsync(scheduleName, scheduleId);
            return Ok(result);
        }

        [HttpPost("update/schedule={scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> EditSchedule(int scheduleId, string color, bool visibility)
        {
            var result = await _scheduleService.EditScheduleAsync(scheduleId, color, visibility);
            return Ok(result);
        }

        [HttpPost("add-user/schedule={scheduleId}/user={userId}")]
        public async Task<ActionResult<ServiceResponse<List<ScheduleOutput>>>> AddUserToSchedule(int scheduleId, int userId)
        {
            var result = await _scheduleService.AddUserToScheduleAsync(scheduleId, userId);
            return Ok(result);
        }
    }
}