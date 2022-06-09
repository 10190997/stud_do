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
        /// Получение Id авторизованного пользователя
        /// </summary>
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<RoomOutput>> GetRoomAsync(int roomId)
        {
            var usersRooms = await GetRoomsAsync();
            var response = new ServiceResponse<RoomOutput>();
            var room = usersRooms.Data.Find(ur => ur.Id == roomId);

            if (room == null)
            {
                response.Success = false;
                response.Message = "Комната не найдена.";
            }

            response.Data = room;

            return response;
        }

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
                                var roleOfUser = await _context.Roles.Where(r => r.Id == roomRole.RoleId).FirstOrDefaultAsync();
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

        public async Task<ServiceResponse<List<RoomOutput>>> GetRoomsAsync()
        {
            var uRooms = await GetRoomsForUser();
            var response = new ServiceResponse<List<RoomOutput>>();
            if (uRooms.Count < 1)
            {
                response.Success = false;
                response.Message = "Не найдено ни одной комнаты.";
            }
            response.Data = uRooms;

            return response;
        }

        public async Task<ServiceResponse<List<string>>> GetRoomSearchSuggestionsAsync(string searchText)
        {
            var rooms = await FindRoomBySearchTextAsync(searchText);

            var results = new List<string>();

            foreach (var room in rooms)
            {
                if (room.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(room.Name);
                }
            }

            return new ServiceResponse<List<string>>
            {
                Data = results
            };
        }

        public async Task<ServiceResponse<List<RoomOutput>>> SearchRoomsAsync(string searchText)
        {
            var rooms = await FindRoomBySearchTextAsync(searchText);

            var response = new ServiceResponse<List<RoomOutput>>
            {
                Data = rooms
            };

            return response;
        }

        /// <summary>
        /// Найти комнаты по названию
        /// </summary>
        /// <param name="searchText">Строка поиска</param>
        private async Task<List<RoomOutput>> FindRoomBySearchTextAsync(string searchText)
        {
            var uRooms = await GetRoomsForUser();
            return uRooms.Where(r => r.Name.ToLower().Contains(searchText.ToLower())).ToList();
        }

        public async Task<ServiceResponse<List<RoomOutput>>> DeleteRoom(int roomId)
        {
            var response = await AdminCheck(roomId);
            if (!response.Success)
            {
                return response;
            }

            var dbRoom = await _context.Rooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
            if (dbRoom == null)
            {
                response.Success = false;
                response.Message = "Комната не найдена.";
                return response;
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
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return await GetRoomsAsync();
        }

        public async Task<ServiceResponse<List<RoomOutput>>> AddRoomAsync(string name)
        {
            var result = new ServiceResponse<List<RoomOutput>>();
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
                result.Success = false;
                result.Message = ex.Message;
                return result;
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
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return await GetRoomsAsync();
        }

        /// <summary>
        /// Проверка прав администратора
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        private async Task<ServiceResponse<List<RoomOutput>>> AdminCheck(int roomId)
        {
            var response = new ServiceResponse<List<RoomOutput>>();
            var urs = await _context.UsersRooms.Where(ur => ur.UserId == GetUserId()).ToListAsync();
            if (urs.Count == 0)
            {
                response.Message = "User has no rooms.";
                response.Success = false;
                return response;
            }
            var ur = urs.FirstOrDefault(ur => ur.RoomId == roomId);
            if (ur == null)
            {
                response.Message = "Комната не найдена.";
                response.Success = false;
            }
            else if (ur.RoleId != 1)
            {
                response.Message = "Нет прав.";
                response.Success = false;
            }
            else
            {
                response.Success = true;
            }
            return response;
        }

        public async Task<ServiceResponse<RoomOutput>> UpdateRoomAsync(string newName, int roomId)
        {
            var response = await AdminCheck(roomId);
            var result = new ServiceResponse<RoomOutput>();
            if (!response.Success)
            {
                result.Success = false;
                result.Message = response.Message;
                return result;
            }

            var dbRoom = await _context.Rooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
            dbRoom.Name = newName;
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

            return await GetRoomAsync(dbRoom.Id);
        }

        public async Task<ServiceResponse<RoomOutput>> AddMemberAsync(int roomId, int userId)
        {
            var response = new ServiceResponse<RoomOutput>();
            var room = await _context.Rooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
            if (room == null)
            {
                response.Success = false;
                response.Message = "Комната не найдена.";
                return response;
            }
            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                response.Success = false;
                response.Message = "Пользователь не найден.";
                return response;
            }
            var ur = GetMember(roomId, userId);
            if (ur.UserId != 0)
            {
                response.Success = false;
                response.Message = "Пользователь уже является участником комнаты";
                return response;
            }

            ur.RoleId = 3;
            ur.UserId = userId;
            ur.RoomId = roomId;

            await _context.UsersRooms.AddAsync(ur);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return await GetRoomAsync(roomId);
        }

        public async Task<ServiceResponse<RoomOutput>> AddModeratorAsync(int roomId, int userId)
        {
            var response = new ServiceResponse<RoomOutput>();
            var room = await _context.Rooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
            if (room == null)
            {
                response.Success = false;
                response.Message = "Комната не найдена.";
                return response;
            }
            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                response.Success = false;
                response.Message = "Пользователь не найден.";
                return response;
            }

            var ur = GetMember(roomId, userId);
            if (ur.UserId == 0)
            {
                response.Success = false;
                response.Message = "Пользователь не является участником комнаты";
                return response;
            }

            ur.RoleId = 2;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return await GetRoomAsync(roomId);
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
    }
}