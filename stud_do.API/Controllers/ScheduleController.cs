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

        [HttpGet("get/{scheduleId}")]
        public async Task<ActionResult<ScheduleOutput>> GetSchedule(int scheduleId)
        {
            var result = await _scheduleService.GetScheduleAsync(scheduleId);
            return Ok(result);
        }

        [HttpDelete("delete/{scheduleId}")]
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

        [HttpPut("creator-update/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> UpdateSchedule(string scheduleName, int scheduleId)
        {
            var result = await _scheduleService.UpdateScheduleAsync(scheduleName, scheduleId);
            return Ok(result);
        }

        [HttpPost("add/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ScheduleOutput>>>> AddSchedule(int scheduleId)
        {
            var result = await _scheduleService.AddScheduleAsync(scheduleId);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<ActionResult<ServiceResponse<ScheduleOutput>>> EditSchedule(ScheduleOutput schedule)
        {
            var result = await _scheduleService.EditScheduleAsync(schedule);
            return Ok(result);
        }
    }
}