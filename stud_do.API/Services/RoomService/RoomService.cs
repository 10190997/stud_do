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

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<RoomOutput>> GetRoomAsync(int roomId)
        {
            var usersRooms = await GetRoomsAsync();
            var response = new ServiceResponse<RoomOutput>();
            var room = usersRooms.Data.Find(ur => ur.Id == roomId);

            if (room == null)
            {
                response.Success = false;
                response.Message = "Room does not exist or is unavailable.";
            }

            response.Data = room;

            return response;
        }

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
                        var role = await _context.Roles.Where(r => r.Id == userRoom.RoleId).FirstOrDefaultAsync();
                        result.Add(new RoomOutput 
                        { 
                            Id = room.Id,
                            Name = room.Name,
                            Role = role.Name 
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
                response.Message = "No rooms found.";
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

        public async Task<ServiceResponse<RoomSearchResult>> SearchRoomsAsync(string searchText, int page)
        {
            var pageResults = 2f;
            var pageCount = Math.Ceiling((await FindRoomBySearchTextAsync(searchText)).Count / pageResults);
            var uRooms = await GetRoomsForUser();
            var rooms = uRooms.Where(r => r.Name.ToLower().Contains(searchText.ToLower()))
                                .Skip((page - 1) * (int)pageResults)
                                .Take((int)pageResults)
                                .ToList();

            var response = new ServiceResponse<RoomSearchResult>
            {
                Data = new RoomSearchResult
                {
                    Rooms = rooms,
                    CurrentPage = page,
                    Pages = (int)pageCount
                }
            };

            return response;
        }

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
            var ur = await _context.UsersRooms.Where(ur => ur.RoomId == roomId).ToListAsync();
            //_context.UsersRooms.RemoveRange(ur);
            _context.Rooms.Remove(dbRoom);
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

            return await GetRoomsAsync();
        }

        public async Task<ServiceResponse<List<RoomOutput>>> AddRoomAsync(string name)
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
                var result = new ServiceResponse<List<RoomOutput>>();
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
                var result = new ServiceResponse<List<RoomOutput>>();
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return await GetRoomsAsync();
        }

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
                response.Message = "Room not found.";
                response.Success = false;
            }
            else if (ur.RoleId != 1)
            {
                response.Message = "No rights.";
                response.Success = false;
            }
            else
            {
                response.Success = true;
            }
            return response;
        }

        public async Task<ServiceResponse<List<RoomOutput>>> UpdateRoomAsync(Room room)
        {
            var response = await AdminCheck(room.Id);
            if (!response.Success)
            {
                return response;
            }

            var dbRoom = await _context.Rooms.Where(r => r.Id == room.Id).FirstOrDefaultAsync();
            dbRoom.Name = room.Name;
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
            // TODO: return ONE room
            return await GetRoomsAsync();
        }
    }
}