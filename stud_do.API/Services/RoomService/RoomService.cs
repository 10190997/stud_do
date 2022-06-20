using System.Security.Claims;

namespace stud_do.API.Services.RoomService
{
    public class RoomService : IRoomService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoomService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Получить комнату
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        public async Task<ServiceResponse<RoomOutput>> GetRoom(int roomId)
        {
            var usersRooms = await GetRooms();
            if (usersRooms.Data == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Нет доступа к комнате"
                };
            }

            var room = usersRooms.Data.Find(ur => ur.Id == roomId);
            if (room == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Комната не найдена."
                };
            }

            return new ServiceResponse<RoomOutput>
            {
                Data = room
            };
        }

        /// <summary>
        /// Создать комнату
        /// </summary>
        /// <param name="name">Название</param>
        public async Task<ServiceResponse<RoomOutput>> CreateRoom(string name)
        {
            Room room = new()
            {
                Name = name
            };
            await _context.Rooms.AddAsync(room);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            UserRoom ur = new()
            {
                RoomId = room.Id,
                UserId = GetUserId(),
                RoleId = 1
            };
            await _context.UsersRooms.AddAsync(ur);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }
            return await GetRoom(room.Id);
        }

        /// <summary>
        /// Вывести список комнат, доступных пользователю
        /// </summary>
        public async Task<ServiceResponse<List<RoomOutput>>> GetRooms()
        {
            var uRooms = await GetRoomsForUser();
            if (uRooms.Count < 1)
            {
                return new ServiceResponse<List<RoomOutput>>
                {
                    Success = false,
                    Message = "Не найдено ни одной комнаты."
                };
            }

            return new ServiceResponse<List<RoomOutput>>
            {
                Data = uRooms
            };
        }

        /// <summary>
        /// Изменить комнату
        /// </summary>
        /// <param name="room">Комната</param>
        public async Task<ServiceResponse<RoomOutput>> UpdateRoom(int roomId, string newName)
        {
            var dbRoom = await _context.Rooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
            if (dbRoom == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Комната не найдена"
                };
            }

            var response = await AdminCheck(roomId);
            if (!response.Success)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = response.Message
                };
            }

            dbRoom.Name = newName;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetRoom(dbRoom.Id);
        }

        /// <summary>
        /// Удалить комнату
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        public async Task<ServiceResponse<List<RoomOutput>>> DeleteRoom(int roomId)
        {
            var dbRoom = await _context.Rooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
            if (dbRoom == null)
            {
                return new ServiceResponse<List<RoomOutput>>
                {
                    Success = false,
                    Message = "Комната не найдена"
                };
            }

            var response = await AdminCheck(roomId);
            if (!response.Success)
            {
                return new ServiceResponse<List<RoomOutput>>
                {
                    Success = false,
                    Message = response.Message
                };
            }

            var ur = await _context.UsersRooms.Where(ur => ur.RoomId == roomId).ToListAsync();
            try
            {
                _context.UsersRooms.RemoveRange(ur);
                _context.Rooms.Remove(dbRoom);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<RoomOutput>>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetRooms();
        }

        /// <summary>
        /// Получить список комнат по названию
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        public async Task<ServiceResponse<List<RoomOutput>>> SearchRooms(string searchText)
        {
            var rooms = await FindRoomBySearchText(searchText);

            if (rooms.Count == 0)
            {
                return new ServiceResponse<List<RoomOutput>>
                {
                    Success = false,
                    Message = "Комнаты не найдены."
                };
            }

            return new ServiceResponse<List<RoomOutput>>
            {
                Data = rooms
            };
        }

        /// <summary>
        /// Получить поисковые подсказки
        /// </summary>
        /// <param name="searchText">Текст поиска</param>
        public async Task<ServiceResponse<List<string>>> GetRoomSearchSuggestions(string searchText)
        {
            var rooms = await FindRoomBySearchText(searchText);

            var results = new List<string>();

            foreach (var room in rooms)
            {
                if (room.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(room.Name);
                }
            }

            if (results.Count == 0)
            {
                return new ServiceResponse<List<string>>
                {
                    Success = false,
                    Message = "Комнаты не найдены."
                };
            }

            return new ServiceResponse<List<string>>
            {
                Data = results
            };
        }

        /// <summary>
        /// Добавить участника комнаты
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        public async Task<ServiceResponse<RoomOutput>> AddMember(int roomId, int userId)
        {
            var r = await _context.Rooms.Where(o => o.Id == roomId).FirstOrDefaultAsync();
            if (r == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Комната не найдена"
                };
            }
            var u = await _context.Users.Where(s => s.Id == userId).FirstOrDefaultAsync();
            if (u == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователь не найден"
                };
            }
            var ur = (await GetUserRights(roomId, userId)).Data;
            if (ur != null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователь уже является участником комнаты"
                };
            }

            var newUr = new UserRoom
            {
                RoleId = 3,
                UserId = userId,
                RoomId = roomId
            };

            await _context.UsersRooms.AddAsync(newUr);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetRoom(roomId);
        }

        /// <summary>
        /// Назначить редактора комнаты
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        public async Task<ServiceResponse<RoomOutput>> AddModerator(int roomId, int userId)
        {
            var r = await _context.Rooms.Where(o => o.Id == roomId).FirstOrDefaultAsync();
            if (r == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Комната не найдена"
                };
            }
            var u = await _context.Users.Where(s => s.Id == userId).FirstOrDefaultAsync();
            if (u == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователь не найден"
                };
            }
            var ur = await GetUserRights(roomId, userId);
            if (ur.Data == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ur.Message
                };
            }
            if (ur.Data.RoleId != 3)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователя нельзя назначить редактором."
                };
            }

            ur.Data.RoleId = 2;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetRoom(roomId);
        }

        public async Task<ServiceResponse<RoomOutput>> RemoveMember(int roomId, int userId)
        {
            var r = await _context.Rooms.Where(o => o.Id == roomId).FirstOrDefaultAsync();
            if (r == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Комната не найдена"
                };
            }
            var u = await _context.Users.Where(s => s.Id == userId).FirstOrDefaultAsync();
            if (u == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователь не найден."
                };
            }
            var ur = await _context.UsersRooms.Where(u => u.UserId == userId && u.RoomId == roomId).FirstOrDefaultAsync();

            if (ur == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователь не является участником комнаты."
                };
            }

            if (ur.UserId != GetUserId())
            {
                var role = await _context.UsersRooms.Where(u => u.UserId == GetUserId() && u.RoomId == roomId).FirstOrDefaultAsync();
                if (role == null)
                {
                    return new ServiceResponse<RoomOutput>
                    {
                        Success = false,
                        Message = "Нет доступа."
                    };
                }
                if (role.RoleId != 1)
                {
                    return new ServiceResponse<RoomOutput>
                    {
                        Success = false,
                        Message = "Нет прав."
                    };
                }
            }
            if (ur.RoleId == 1)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Администратор не может покинуть комнату."
                };
            }
            _context.UsersRooms.Remove(ur);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetRoom(roomId);
        }

        public async Task<ServiceResponse<RoomOutput>> RemoveModerator(int roomId, int userId)
        {
            var r = await _context.Rooms.Where(o => o.Id == roomId).FirstOrDefaultAsync();
            if (r == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Комната не найдена"
                };
            }
            var u = await _context.Users.Where(s => s.Id == userId).FirstOrDefaultAsync();
            if (u == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователь не найден"
                };
            }
            var ur = await GetUserRights(roomId, userId);
            if (ur.Data == null)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ur.Message
                };
            }
            if (ur.Data.RoleId != 2)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = "Пользователь не является редактором."
                };
            }
            ur.Data.RoleId = 3;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoomOutput>
                {
                    Success = false,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException.Message
                };
            }

            return await GetRoom(roomId);
        }

        #region private

        private async Task<ServiceResponse<UserRoom>> GetUserRights(int roomId, int userId)
        {
            var response = await AdminCheck(roomId);
            if (!response.Success)
            {
                return new ServiceResponse<UserRoom>
                {
                    Success = false,
                    Message = "Нет прав."
                };
            }

            var ur = GetMember(roomId, userId);
            if (ur.UserId == 0)
            {
                return new ServiceResponse<UserRoom>
                {
                    Success = false,
                    Message = "Пользователь не является участником комнаты"
                };
            }
            else
            {
                return new ServiceResponse<UserRoom>
                {
                    Data = ur
                };
            }
        }

        /// <summary>
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// Получить комнаты, доступные пользователю
        /// </summary>
        private async Task<List<RoomOutput>> GetRoomsForUser()
        {
            var usersRooms = await _context.UsersRooms.Where(ur => ur.UserId == GetUserId()).ToListAsync();
            var result = new List<RoomOutput>();
            if (usersRooms.Count != 0)
            {
                var rooms = await _context.Rooms.ToListAsync();

                foreach (var room in rooms)
                {
                    var userRoom = usersRooms.FirstOrDefault(ur => ur.RoomId == room.Id);
                    if (userRoom != null)
                    {
                        var roomsForUsers = await _context.UsersRooms.Where(ur => ur.RoomId == room.Id).ToListAsync();
                        var users = new List<UserRoomOutput>();
                        foreach (var user in roomsForUsers)
                        {
                            var u = _context.Users.Where(u => u.Id == user.UserId).FirstOrDefault();
                            if (u != null)
                            {
                                var roomRole = await _context.UsersRooms.Where(ur => ur.UserId == u.Id && ur.RoomId == room.Id).FirstOrDefaultAsync();
                                if (roomRole == null)
                                {
                                    return result;
                                }
                                var roleOfUser = await _context.Roles.Where(r => r.Id == roomRole.RoleId).FirstOrDefaultAsync();
                                if (roleOfUser == null)
                                {
                                    return result;
                                }
                                users.Add(new UserRoomOutput
                                {
                                    Id = u.Id,
                                    Email = u.Email,
                                    Login = u.Login,
                                    Role = roleOfUser.Name
                                });
                            }
                        }
                        var role = await _context.Roles.Where(r => r.Id == userRoom.RoleId).FirstOrDefaultAsync();
                        if (role == null)
                        {
                            return result;
                        }
                        result.Add(new RoomOutput
                        {
                            Id = room.Id,
                            Name = room.Name,
                            Role = role.Name,
                            Users = users
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Найти комнаты по названию
        /// </summary>
        /// <param name="searchText">Строка поиска</param>
        private async Task<List<RoomOutput>> FindRoomBySearchText(string searchText)
        {
            var uRooms = await GetRoomsForUser();
            return uRooms.Where(r => r.Name.ToLower().Contains(searchText.ToLower())).ToList();
        }

        /// <summary>
        /// Получить участника комнаты
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        private UserRoom GetMember(int roomId, int userId)
        {
            var users = _context.UsersRooms.Where(x => x.UserId == userId).ToList();
            var ur = users.Where(x => x.RoomId == roomId).FirstOrDefault();
            if (ur == null)
            {
                return new UserRoom();
            }
            return ur;
        }

        /// <summary>
        /// Проверка прав администратора
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        private async Task<ServiceResponse<bool>> AdminCheck(int roomId)
        {
            var urs = await _context.UsersRooms.Where(ur => ur.UserId == GetUserId()).ToListAsync();
            if (urs.Count == 0)
            {
                return new ServiceResponse<bool>
                {
                    Message = "Комнаты не найдены.",
                    Success = false
                };
            }
            var ur = urs.FirstOrDefault(ur => ur.RoomId == roomId);
            if (ur == null)
            {
                return new ServiceResponse<bool>
                {
                    Message = "Комната не найдена.",
                    Success = false
                };
            }
            else if (ur.RoleId != 1)
            {
                return new ServiceResponse<bool>
                {
                    Message = "Нет прав.",
                    Success = false
                };
            }
            return new ServiceResponse<bool>();
        }

        #endregion private
    }
}