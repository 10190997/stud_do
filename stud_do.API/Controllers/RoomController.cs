using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Services;
using stud_do.API.Services.RoomService;

namespace stud_do.API.Controllers
{
    [EnableCors("_myAllowSpecificOrigins")]
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

        [HttpGet("get-all")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> GetRooms()
        {
            var result = await _roomService.GetRoomsAsync();
            return Ok(result);
        }

        [HttpGet("get/room={roomId}")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> GetRoom(int roomId)
        {
            var result = await _roomService.GetRoomAsync(roomId);
            return Ok(result);
        }

        [HttpGet("search/text={searchText}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> SearchRooms(string searchText)
        {
            var result = await _roomService.SearchRoomsAsync(searchText);
            return Ok(result);
        }

        [HttpGet("searchsuggestions/text={searchText}")]
        public async Task<ActionResult<ServiceResponse<List<string>>>> GetRoomSearchSuggestions(string searchText)
        {
            var result = await _roomService.GetRoomSearchSuggestionsAsync(searchText);
            return Ok(result);
        }

        [HttpPost("delete/room={roomId}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> DeleteRoom(int roomId)
        {
            var result = await _roomService.DeleteRoom(roomId);
            return Ok(result);
        }

        [HttpPost("create/name={roomName}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> AddRoom(string roomName)
        {
            var result = await _roomService.AddRoomAsync(roomName);
            return Ok(result);
        }

        [HttpPost("update/room={roomId}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> UpdateRoom(string newName, int roomId)
        {
            var result = await _roomService.UpdateRoomAsync(newName, roomId);
            return Ok(result);
        }

        [HttpPost("add-member/room={roomId}/user={userId}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> AddMember(int roomId, int userId)
        {
            var result = await _roomService.AddMemberAsync(roomId, userId);
            return Ok(result);
        }

        [HttpPost("add-moderator/room={roomId}/user={userId}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> AddModerator(int roomId, int userId)
        {
            var result = await _roomService.AddModeratorAsync(roomId, userId);
            return Ok(result);
        }
    }
}