using System.Security.Claims;

namespace stud_do.API.Services.PostService
{
    public class PostService : IPostService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<PostOutput>>> AddPostAsync(PostInput post)
        {
            var result = new ServiceResponse<List<PostOutput>>();
            if (!IsAdmin(post.RoomId) && !IsModerator(post.RoomId))
            {
                result.Success = false;
                result.Message = "No rights";
                return result;
            }
            Post newPost = new()
            {
                RoomId = post.RoomId,
                Text = post.Text,
                Attachments = post.Attachments
            };
            _context.Posts.Add(newPost);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return await GetPostsAsync(newPost.RoomId);
        }

        private bool IsAdmin(int roomId)
        {
            var room = _context.UsersRooms.Where(r => r.UserId == GetUserId() && r.RoomId == roomId).FirstOrDefaultAsync();
            return room.Result.RoleId == 1;
        }

        private bool IsModerator(int roomId)
        {
            var room = _context.UsersRooms.Where(r => r.UserId == GetUserId() && r.RoomId == roomId).FirstOrDefaultAsync();
            return room.Result.RoleId == 2;
        }

        public async Task<ServiceResponse<List<PostOutput>>> DeletePostAsync(int postId)
        {
            var result = new ServiceResponse<List<PostOutput>>();
            var dbPost = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (!IsAdmin(dbPost.RoomId) && !IsModerator(dbPost.RoomId))
            {
                result.Success = false;
                result.Message = "No rights";
                return result;
            }
            _context.Posts.Remove(dbPost);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return await GetPostsAsync(dbPost.RoomId);
        }

        public async Task<ServiceResponse<PostOutput>> GetPostAsync(int postId)
        {
            var result = new ServiceResponse<PostOutput>();
            var post = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            PostOutput output = new()
            {
                Text = post.Text,
                RoomId = post.RoomId,
                Id = postId
            };
            result.Data = output;
            return result;
        }

        public async Task<ServiceResponse<List<PostOutput>>> GetPostsAsync(int roomId)
        {
            var result = new ServiceResponse<List<PostOutput>>();
            var posts = await _context.Posts.Where(p => p.RoomId == roomId).ToListAsync();
            var outputs = new List<PostOutput>();
            foreach (var post in posts)
            {
                PostOutput output = new()
                {
                    Text = post.Text,
                    RoomId = post.RoomId,
                    Id = post.Id
                };
                outputs.Add(output);
            }
            result.Data = outputs;
            return result;
        }

        public async Task<ServiceResponse<PostOutput>> UpdatePostAsync(Post post)
        {
            var result = new ServiceResponse<PostOutput>();
            if (!IsAdmin(post.RoomId) && !IsModerator(post.RoomId))
            {
                result.Success = false;
                result.Message = "No rights";
                return result;
            }
            var dbPost = await _context.Posts.Where(p => p.Id == post.Id).FirstOrDefaultAsync();
            dbPost = post;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            PostOutput output = new()
            {
                Text = post.Text,
                RoomId = post.RoomId,
                Id = post.Id
            };
            result.Data = output;
            return result;
        }
    }
}