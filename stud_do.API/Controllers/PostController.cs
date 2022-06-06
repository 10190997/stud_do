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

        [HttpGet("get-all/{roomId}")]
        public async Task<ActionResult<ServiceResponse<List<PostOutput>>>> GetPosts(int roomId)
        {
            var result = await _postService.GetPostsAsync(roomId);
            return Ok(result);
        }

        [HttpGet("get/{postId}")]
        public async Task<ActionResult<ServiceResponse<List<PostOutput>>>> GetPost(int postId)
        {
            var result = await _postService.GetPostAsync(postId);
            return Ok(result);
        }

        [HttpDelete("delete/{postId}")]
        public async Task<ActionResult<ServiceResponse<List<PostOutput>>>> DeletePost(int postId)
        {
            var result = await _postService.DeletePostAsync(postId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<List<PostOutput>>>> AddPost(PostInput post)
        {
            var result = await _postService.AddPostAsync(post);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<ActionResult<ServiceResponse<List<PostOutput>>>> UpdatePost(Post post)
        {
            var result = await _postService.UpdatePostAsync(post);
            return Ok(result);
        }
    }
}