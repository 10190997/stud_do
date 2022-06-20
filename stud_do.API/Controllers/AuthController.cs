using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Services;
using stud_do.API.Services.AuthService;
using System.Security.Claims;

namespace stud_do.API.Controllers
{
    [EnableCors("_myAllowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegister request)
        {
            var response = await _authService.Register(
                new User
                {
                    Email = request.Email,
                    Login = request.Login
                },
                request.Password);

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServiceResponse<string>>> Login(UserLogin request)
        {
            var response = await _authService.Login(request.Email, request.Password);

            return Ok(response);
        }

        [HttpPost("change-password"), Authorize]
        public async Task<ActionResult<ServiceResponse<bool>>> ChangePassword([FromBody]string newPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _authService.ChangePassword(int.Parse(userId), newPassword);

            return Ok(response);
        }
    }
}