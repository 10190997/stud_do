using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using stud_do.API.Services;
using stud_do.API.Services.PostService;

namespace stud_do.API.Controllers
{
    [EnableCors("_myAllowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("get-all/room={roomId}")]
        public async Task<ActionResult<ServiceResponse<List<PostOutput>>>> GetPosts(int roomId)
        {
            var result = await _postService.GetPosts(roomId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<PostOutput>>> CreatePost(PostInput post)
        {
            var result = await _postService.CreatePost(post);
            return Ok(result);
        }

        [HttpGet("get/{postId}")]
        public async Task<ActionResult<ServiceResponse<PostOutput>>> GetPost(int postId)
        {
            var result = await _postService.GetPost(postId);
            return Ok(result);
        }

        [HttpPost("update/{postId}")]
        public async Task<ActionResult<ServiceResponse<PostOutput>>> UpdatePost(PostInput post, int postId)
        {
            var result = await _postService.UpdatePost(postId, post);
            return Ok(result);
        }

        [HttpPost("delete/{postId}")]
        public async Task<ActionResult<ServiceResponse<List<PostOutput>>>> DeletePost(int postId)
        {
            var result = await _postService.DeletePost(postId);
            return Ok(result);
        }
    }
}