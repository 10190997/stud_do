using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Services;
using stud_do.API.Services.RoomService;

namespace stud_do.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // TODO: getroomS
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> GetRoom()
        {
            var result = await _roomService.GetRoomsAsync();
            return Ok(result);
        }

        // TODO: not a List
        [HttpGet("{roomId}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> GetRoom(int roomId)
        {
            var result = await _roomService.GetRoomAsync(roomId);
            return Ok(result);
        }

        [HttpGet("search/{searchText}/{page}")]
        public async Task<ActionResult<ServiceResponse<RoomSearchResult>>> SearchRooms(string searchText, int page = 1)
        {
            var result = await _roomService.SearchRoomsAsync(searchText, page);
            return Ok(result);
        }

        [HttpGet("searchsuggestions/{searchText}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> GetRoomSearchSuggestions(string searchText)
        {
            var result = await _roomService.GetRoomSearchSuggestionsAsync(searchText);
            return Ok(result);
        }

        [HttpDelete("{roomId}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> DeleteRoom(int roomId)
        {
            var result = await _roomService.DeleteRoom(roomId);
            return Ok(result);
        }

        [HttpPost("{roomName}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> AddRoom(string roomName)
        {
            var result = await _roomService.AddRoomAsync(roomName);
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> UpdateRoom(Room room)
        {
            var result = await _roomService.UpdateRoomAsync(room);
            return Ok(result);
        }
    }
}