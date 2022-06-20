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
            var result = await _scheduleService.GetSchedules();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> CreateSchedule(ScheduleInput schedule)
        {
            var result = await _scheduleService.CreateSchedule(schedule);
            return Ok(result);
        }

        [HttpGet("get/{scheduleId}")]
        public async Task<ActionResult<ScheduleOutput>> GetSchedule(int scheduleId)
        {
            var result = await _scheduleService.GetSchedule(scheduleId);
            return Ok(result);
        }

        [HttpPost("update-name/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> UpdateScheduleName(int scheduleId, [FromBody] string scheduleName)
        {
            var result = await _scheduleService.UpdateScheduleName(scheduleId, scheduleName);
            return Ok(result);
        }

        [HttpPost("update-color/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> UpdateScheduleColor(int scheduleId, [FromBody] string color)
        {
            var result = await _scheduleService.UpdateScheduleColor(scheduleId, color);
            return Ok(result);
        }

        [HttpPost("delete/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ScheduleOutput>>>> DeleteSchedule(int scheduleId)
        {
            var result = await _scheduleService.DeleteSchedule(scheduleId);
            return Ok(result);
        }

        [HttpPost("show/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> ShowSchedule(int scheduleId)
        {
            var result = await _scheduleService.ShowSchedule(scheduleId);
            return Ok(result);
        }

        [HttpPost("hide/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> HideSchedule(int scheduleId)
        {
            var result = await _scheduleService.HideSchedule(scheduleId);
            return Ok(result);
        }

        [HttpPost("add-user/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> AddUserToSchedule(int scheduleId, [FromBody] int userId)
        {
            var result = await _scheduleService.AddUserToSchedule(scheduleId, userId);
            return Ok(result);
        }

        [HttpPost("remove-user/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> RemoveUserFromSchedule(int scheduleId, [FromBody] int userId)
        {
            var result = await _scheduleService.RemoveUserFromSchedule(scheduleId, userId);
            return Ok(result);
        }
    }
}