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
            var result = await _roomService.GetRooms();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> CreateRoom(string roomName)
        {
            var result = await _roomService.CreateRoom(roomName);
            return Ok(result);
        }

        [HttpGet("get/{roomId}")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> GetRoom(int roomId)
        {
            var result = await _roomService.GetRoom(roomId);
            return Ok(result);
        }

        [HttpPost("update/{roomId}")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> UpdateRoom(int roomId, string newName)
        {
            var result = await _roomService.UpdateRoom(roomId, newName);
            return Ok(result);
        }

        [HttpPost("delete/{roomId}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> DeleteRoom(int roomId)
        {
            var result = await _roomService.DeleteRoom(roomId);
            return Ok(result);
        }

        [HttpGet("search/{searchText}")]
        public async Task<ActionResult<ServiceResponse<List<RoomOutput>>>> SearchRooms(string searchText)
        {
            var result = await _roomService.SearchRooms(searchText);
            return Ok(result);
        }

        [HttpGet("searchsuggestions/{searchText}")]
        public async Task<ActionResult<ServiceResponse<List<string>>>> GetRoomSearchSuggestions(string searchText)
        {
            var result = await _roomService.GetRoomSearchSuggestions(searchText);
            return Ok(result);
        }

        [HttpPost("add-member/{roomId}")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> AddMember(int roomId, int userId)
        {
            var result = await _roomService.AddMember(roomId, userId);
            return Ok(result);
        }

        [HttpPost("remove-member/{roomId}")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> RemoveMember(int roomId, int userId)
        {
            var result = await _roomService.RemoveMember(roomId, userId);
            return Ok(result);
        }

        [HttpPost("add-moderator/{roomId}")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> AddModerator(int roomId, int userId)
        {
            var result = await _roomService.AddModerator(roomId, userId);
            return Ok(result);
        }

        [HttpPost("remove-moderator/{roomId}")]
        public async Task<ActionResult<ServiceResponse<RoomOutput>>> RemoveModerator(int roomId, int userId)
        {
            var result = await _roomService.RemoveModerator(roomId, userId);
            return Ok(result);
        }
    }
}