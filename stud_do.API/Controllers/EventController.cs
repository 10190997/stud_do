using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Services;
using stud_do.API.Services.EventService;

namespace stud_do.API.Controllers
{
    [EnableCors("_myAllowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("get-all/schedule={scheduleId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> GetEvents(int scheduleId)
        {
            var result = await _eventService.GetEvents(scheduleId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<EventOutput>>> CreateEvent(EventInput ev)
        {
            var result = await _eventService.CreateEvent(ev);
            return Ok(result);
        }

        [HttpGet("get/{eventId}")]
        public async Task<ActionResult<ServiceResponse<EventOutput>>> GetEvent(int eventId)
        {
            var result = await _eventService.GetEvent(eventId);
            return Ok(result);
        }

        [HttpPost("update/{eventId}")]
        public async Task<ActionResult<ServiceResponse<EventOutput>>> UpdateEvent(int eventId, EventInput ev)
        {
            var result = await _eventService.UpdateEvent(eventId, ev);
            return Ok(result);
        }

        [HttpPost("delete/{eventId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> DeleteEvent(int eventId)
        {
            var result = await _eventService.DeleteEvent(eventId);
            return Ok(result);
        }
    }
}