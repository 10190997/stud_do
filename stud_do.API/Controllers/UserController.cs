using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Model.DTOs.User;
using stud_do.API.Services;
using stud_do.API.Services.UserService;

namespace stud_do.API.Controllers
{
    [EnableCors("_myAllowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get")]
        public async Task<ActionResult<ServiceResponse<UserOutput>>> Get()
        {
            var result = await _userService.GetUser();
            return Ok(result);
        }

        [HttpPost("update")]
        public async Task<ActionResult<ServiceResponse<UserOutput>>> Update(UserInput user)
        {
            var result = await _userService.UpdateUser(user);
            return Ok(result);
        }

        [HttpPost("delete")]
        public async Task<ActionResult<bool>> Delete()
        {
            var result = await _userService.DeleteUser();
            return Ok(result);
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ServiceResponse<UserOutput>>> GetAll()
        {
            var result = await _userService.GetAllUsers();
            return Ok(result);
        }

        [HttpGet("search/text={searchText}")]
        public async Task<ActionResult<ServiceResponse<List<UserOutput>>>> SearchUsers(string searchText)
        {
            var result = await _userService.SearchUsersAsync(searchText);
            return Ok(result);
        }

        [HttpGet("searchsuggestions/text={searchText}")]
        public async Task<ActionResult<ServiceResponse<List<string>>>> GetUserSearchSuggestions(string searchText)
        {
            var result = await _userService.GetUserSearchSuggestionsAsync(searchText);
            return Ok(result);
        }
    }
}