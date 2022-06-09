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
            var result = await _eventService.GetEventsAsync(scheduleId);
            return Ok(result);
        }

        [HttpGet("get/event={eventId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> GetEvent(int eventId)
        {
            var result = await _eventService.GetEventAsync(eventId);
            return Ok(result);
        }

        [HttpPost("delete/event={eventId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> DeleteEvent(int eventId)
        {
            var result = await _eventService.DeleteEventAsync(eventId);
            return Ok(result);
        }

        [HttpPost("add-event/schedule={scheduleId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> AddEvent(EventInput ev, int scheduleId)
        {
            var result = await _eventService.AddEventAsync(ev, scheduleId);
            return Ok(result);
        }

        [HttpPost("update/event={eventId}")]
        public async Task<ActionResult<ServiceResponse<List<EventOutput>>>> UpdateEvent(EventInput ev, int eventId)
        {
            var result = await _eventService.UpdateEventAsync(ev, eventId);
            return Ok(result);
        }
    }
}