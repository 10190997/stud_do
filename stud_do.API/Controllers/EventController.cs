using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Services;
using stud_do.API.Services.EventService;

namespace stud_do.API.Controllers
{
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

        [HttpGet("get-all/{scheduleId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> GetEvents(int scheduleId)
        {
            var result = await _eventService.GetEventsAsync(scheduleId);
            return Ok(result);
        }

        [HttpGet("get/{eventId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> GetEvent(int eventId)
        {
            var result = await _eventService.GetEventAsync(eventId);
            return Ok(result);
        }

        [HttpDelete("delete/{eventId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> DeleteEvent(int eventId)
        {
            var result = await _eventService.DeleteEventAsync(eventId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> AddEvent(EventInput ev)
        {
            var result = await _eventService.AddEventAsync(ev);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> UpdateEvent(Event ev)
        {
            var result = await _eventService.UpdateEventAsync(ev);
            return Ok(result);
        }
    }
}