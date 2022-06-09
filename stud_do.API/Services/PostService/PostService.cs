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

        /// <summary>
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<PostOutput>>> AddPostAsync(PostInput post, int roomId)
        {
            var result = new ServiceResponse<List<PostOutput>>();
            var dbRoom = await _context.Rooms.Where(p => p.Id == roomId).FirstOrDefaultAsync();
            if (dbRoom == null)
            {
                result.Success = false;
                result.Message = "Комната не найдена.";
                return result;
            }
            if (!IsAdmin(roomId) && !IsModerator(roomId))
            {
                result.Success = false;
                result.Message = "Нет прав на добавление записи.";
                return result;
            }
            Post newPost = new()
            {
                RoomId = roomId,
                Text = post.Text
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
            if (post.Attachments != null)
            {
                List<Attachment> atts = new();
                foreach (var item in post.Attachments)
                {
                    atts.Add(new Attachment { Link = item, PostId = newPost.Id });
                }
                _context.Attachments.AddRange(atts);
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
            }
            return await GetPostsAsync(newPost.RoomId);
        }

        /// <summary>
        /// Проверка наличия прав администратора
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        private bool IsAdmin(int roomId)
        {
            var room = _context.UsersRooms.Where(r => r.UserId == GetUserId() && r.RoomId == roomId).FirstOrDefault();
            if (room == null)
            {
                return false;
            }
            return room.RoleId == 1;
        }

        /// <summary>
        /// Проверка наличия прав редактора
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        private bool IsModerator(int roomId)
        {
            var room = _context.UsersRooms.Where(r => r.UserId == GetUserId() && r.RoomId == roomId).FirstOrDefault();
            if (room == null)
            {
                return false;
            }
            return room.RoleId == 2;
        }

        public async Task<ServiceResponse<List<PostOutput>>> DeletePostAsync(int postId)
        {
            var result = new ServiceResponse<List<PostOutput>>();
            var dbPost = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (dbPost == null)
            {
                result.Success = false;
                result.Message = "Запись не найдена.";
                return result;
            }
            if (!IsAdmin(dbPost.RoomId) && !IsModerator(dbPost.RoomId))
            {
                result.Success = false;
                result.Message = "Нет прав на удаление записи.";
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
            if (post == null)
            {
                result.Success = false;
                result.Message = "Запись не найдена.";
                return result;
            }
            var roomUsrs = await _context.UsersRooms.Where(p => p.RoomId == post.RoomId).ToListAsync();
            if (!roomUsrs.Any(x => x.UserId == GetUserId()))
            {
                result.Success = false;
                result.Message = "Нет доступа к комнате.";
                return result;
            }
            var atts = await GetAttahcments(postId);
            PostOutput output = new()
            {
                Text = post.Text,
                RoomId = post.RoomId,
                Id = postId,
                Attachments = atts
            };
            result.Data = output;
            return result;
        }

        /// <summary>
        /// Получить вложения в записи
        /// </summary>
        /// <param name="postId">Id записи</param>
        /// <returns></returns>
        private async Task<List<string>> GetAttahcments(int postId)
        {
            var dbAtts = await _context.Attachments.Where(a => a.PostId == postId).ToListAsync();
            var atts = new List<string>();
            if (dbAtts != null)
            {
                foreach (var item in dbAtts)
                {
                    atts.Add(item.Link);
                }
            }
            return atts;
        }

        public async Task<ServiceResponse<List<PostOutput>>> GetPostsAsync(int roomId)
        {
            var result = new ServiceResponse<List<PostOutput>>();
            var roomUsrs = await _context.UsersRooms.Where(p => p.RoomId == roomId).ToListAsync();
            if (!roomUsrs.Any(x => x.UserId == GetUserId()))
            {
                result.Success = false;
                result.Message = "Нет доступа к комнате.";
                return result;
            }
            var posts = await _context.Posts.Where(p => p.RoomId == roomId).ToListAsync();
            var outputs = new List<PostOutput>();
            foreach (var post in posts)
            {
                PostOutput output = new()
                {
                    Text = post.Text,
                    RoomId = roomId,
                    Id = post.Id,
                    Attachments = await GetAttahcments(post.Id)
                };
                outputs.Add(output);
            }
            result.Data = outputs;
            return result;
        }

        public async Task<ServiceResponse<PostOutput>> UpdatePostAsync(PostInput post, int postId)
        {
            var result = new ServiceResponse<PostOutput>();
            var dbPost = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (dbPost == null)
            {
                result.Success = false;
                result.Message = "Запись не найдена.";
                return result;
            }
            if (!IsAdmin(dbPost.RoomId) && !IsModerator(dbPost.RoomId))
            {
                result.Success = false;
                result.Message = "Нет прав на изменение записи.";
                return result;
            }
            
            dbPost.Text = post.Text;
            var dbAtts = await _context.Attachments.Where(a => a.PostId == postId).ToListAsync();
            if (dbAtts != null)
            {
                _context.Attachments.RemoveRange(dbAtts);
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
            }
            if (post.Attachments != null)
            {
                List<Attachment> atts = new();
                foreach (var item in post.Attachments)
                {
                    atts.Add(new Attachment 
                    { 
                        Link = item, 
                        PostId = dbPost.Id 
                    });
                }

                _context.Attachments.AddRange(atts);
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
            }
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
                RoomId = dbPost.RoomId,
                Id = postId,
                Attachments = await GetAttahcments(dbPost.Id)
            };
            result.Data = output;
            return result;
        }
    }
}