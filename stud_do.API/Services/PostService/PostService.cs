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
        /// Получить все записи в комнате
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        public async Task<ServiceResponse<List<PostOutput>>> GetPosts(int roomId)
        {
            var roomUsrs = await _context.UsersRooms.Where(p => p.RoomId == roomId).ToListAsync();
            if (!roomUsrs.Any(x => x.UserId == GetUserId()))
            {
                return new ServiceResponse<List<PostOutput>>
                {
                    Success = false,
                    Message = "Нет доступа к комнате."
                };
            }
            var posts = await _context.Posts.Where(p => p.RoomId == roomId).ToListAsync();
            if (posts.Count == 0)
            {
                return new ServiceResponse<List<PostOutput>>
                {
                    Success = false,
                    Message = "Записи не найдены."
                };
            }
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
            return new ServiceResponse<List<PostOutput>>
            {
                Data = outputs
            };
        }

        /// <summary>
        /// Создать запись и добавить ее в комнату
        /// </summary>
        /// <param name="post">Запись</param>
        /// <param name="roomId">Id комнаты</param>
        public async Task<ServiceResponse<PostOutput>> CreatePost(PostInput post)
        {
            var dbRoom = await _context.Rooms.Where(p => p.Id == post.RoomId).FirstOrDefaultAsync();
            if (dbRoom == null)
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = "Комната не найдена."
                };
            }
            if (!IsAdmin(post.RoomId) && !IsModerator(post.RoomId))
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = "Нет прав на добавление записи."
                };
            }
            Post newPost = new()
            {
                RoomId = post.RoomId,
                Text = post.Text
            };
            _context.Posts.Add(newPost);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
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
                    return new ServiceResponse<PostOutput>
                    {
                        Success = false,
                        Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                    };
                }
            }
            return await GetPost(newPost.Id);
        }

        /// <summary>
        /// Получить запись
        /// </summary>
        /// <param name="postId">Id записи</param>
        public async Task<ServiceResponse<PostOutput>> GetPost(int postId)
        {
            var post = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = "Запись не найдена."
                };
            }
            var roomUsrs = await _context.UsersRooms.Where(p => p.RoomId == post.RoomId).ToListAsync();
            if (!roomUsrs.Any(x => x.UserId == GetUserId()))
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = "Нет доступа к комнате."
                };
            }
            var atts = await GetAttahcments(postId);
            PostOutput output = new()
            {
                Text = post.Text,
                RoomId = post.RoomId,
                Id = postId,
                Attachments = atts
            };
            return new ServiceResponse<PostOutput>
            {
                Data = output
            };
        }

        /// <summary>
        /// Редактировать запись
        /// </summary>
        /// <param name="post">Запись</param>
        /// <param name="postId">Id записи</param>
        public async Task<ServiceResponse<PostOutput>> UpdatePost(int postId, PostInput post)
        {
            var dbPost = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (dbPost == null)
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = "Запись не найдена."
                };
            }
            if (!IsAdmin(dbPost.RoomId) && !IsModerator(dbPost.RoomId))
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = "Нет прав на изменение записи."
                };
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
                    return new ServiceResponse<PostOutput>
                    {
                        Success = false,
                        Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                    };
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
                    return new ServiceResponse<PostOutput>
                    {
                        Success = false,
                        Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                    };
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<PostOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            PostOutput output = new()
            {
                Text = post.Text,
                RoomId = dbPost.RoomId,
                Id = postId,
                Attachments = await GetAttahcments(dbPost.Id)
            };
            return new ServiceResponse<PostOutput>
            {
                Data = output
            };
        }

        /// <summary>
        /// Удалить запись
        /// </summary>
        /// <param name="postId">Id записи</param>
        public async Task<ServiceResponse<List<PostOutput>>> DeletePost(int postId)
        {
            var dbPost = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (dbPost == null)
            {
                return new ServiceResponse<List<PostOutput>>
                {
                    Success = false,
                    Message = "Запись не найдена."
                };
            }
            if (!IsAdmin(dbPost.RoomId) && !IsModerator(dbPost.RoomId))
            {
                return new ServiceResponse<List<PostOutput>>
                {
                    Success = false,
                    Message = "Нет прав на удаление записи."
                };
            }
            _context.Posts.Remove(dbPost);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<PostOutput>>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            return await GetPosts(dbPost.RoomId);
        }

        #region private

        /// <summary>
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

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

        #endregion private
    }
}