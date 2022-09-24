using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chat.Web.Data;
using Chat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Chat.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Chat.Web.ViewModels;
using System.Diagnostics;

namespace Chat.Web.Controllers
{
    [AllowAnonymous] ///TODO:удалить
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public RoomsController(ApplicationDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> Get()
        {
            // Загружаем связанные сущности для корректной работы
            //var rooms = await _context.Rooms.ToListAsync();
            //var users = await _context.AppUsers.ToListAsync();
            var user = _context.Users.FirstOrDefault(u => u.UserName == "copyright");//User.Identity.Name);
            var userRooms = await _context.ChatMembers
                .Where(u => u.SomeUser == user)
                .Select(u => u.SomeRoom).ToListAsync();
            
            /*List<Room> userRooms = new();

            foreach (ChatMember someChatMember in chatMembers)
            {
                if (someChatMember.SomeUser.Equals(user))
                {
                    userRooms.Add(someChatMember.SomeRoom);
                }
            }*/

            var roomsViewModel = _mapper.Map<IEnumerable<Room>, IEnumerable<RoomViewModel>>(userRooms);
            
            return Ok(roomsViewModel);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);
            return Ok(roomViewModel);
        }

        [HttpPost]
        [Route("private")]
        public async Task<ActionResult<Room>> CreatePrivateChat(PrivateChatView roomViewModel)
        {
            var firstUsername = roomViewModel.Name;
            var firstUser = _context.Users.FirstOrDefault(u => u.UserName == firstUsername);
            var secondUser = _context.Users.FirstOrDefault(u => u.UserName == "copyright");//User.Identity.Name);
            var secondUsername = secondUser.UserName;

            // Create new private room
            Room room; 
            if (firstUsername.CompareTo(secondUsername) < 0)
            {
                if (_context.Rooms.Any(r => r.Name == firstUsername + "<->" + secondUsername))
                    return BadRequest("Приватный чат с этим пользователем уже существует!");

                room = new Room()
                {
                    Name = firstUsername + "_" + secondUsername,
                    Admin = secondUser,
                    Fuul = "stringg"
                };
            } 
            else
            {
                if (_context.Rooms.Any(r => r.Name == secondUsername + "<->" + firstUsername))
                    return BadRequest("Приватный чат с этим пользователем уже существует!");

                room = new Room()
                {
                    Name = secondUsername + "<->" + firstUsername,
                    Admin = secondUser,
                    Fuul = "stringg"
                };
            }
            _context.Rooms.Add(room);

            
            // Create new chat members
            ChatMember firstChatMember = new ChatMember()
            {
                SomeRoom = room,
                SomeUser = firstUser,
                IsPrivate = true
            };
            ChatMember secondChatMember = new ChatMember()
            {
                SomeRoom = room,
                SomeUser = secondUser,
                IsPrivate = true
            };

            if (firstUsername.CompareTo(secondUsername) == 0)
            {
                _context.ChatMembers.Add(firstChatMember);
            }
            else
            {
                _context.ChatMembers.Add(firstChatMember);
                _context.ChatMembers.Add(secondChatMember);
            }
            
            await _context.SaveChangesAsync();
            //await _hubContext.Clients.All.SendAsync("addChatRoom", new { id = room.Id, name = room.Name });

            return CreatedAtAction(nameof(Get), new { id = room.Id }, new { id = room.Id, name = room.Name });
        }

        [HttpPost]
        public async Task<ActionResult<Room>> Create(RoomViewModel roomViewModel)
        {
            if (_context.Rooms.Any(r => r.Name == roomViewModel.Name))
                return BadRequest("Неверное название чата или чат уже существует");

            var user = _context.Users.FirstOrDefault(u => u.UserName == "copyright");//User.Identity.Name);
            var room = new Room()
            {
                Name = roomViewModel.Name,
                Admin = user,
                Fuul = "stringg"
            };

            _context.Rooms.Add(room);

            ChatMember chatMember = new ChatMember()
            {
                SomeRoom = room,
                SomeUser = user,
                IsPrivate = false
            };
            _context.ChatMembers.Add(chatMember);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = room.Id }, new { id = room.Id, name = room.Name });
        }

        [HttpPost]
        [Route("addUser")]
        public async Task<IActionResult> AddUserToChat(PrivateChatView roomViewModel)
        {
            Debug.WriteLine("IM HERERERERERERRERERERERRERERERRERE");
            var room = await _context.Rooms.Where(r => r.Id == roomViewModel.Id).FirstOrDefaultAsync();
            if (room == null)
                return NotFound();
            var user = _context.Users.FirstOrDefault(u => u.UserName == roomViewModel.Name);
            ChatMember chatMember = new ChatMember()
            {
                SomeRoom = room,
                SomeUser = user,
                IsPrivate = false
            };
            _context.ChatMembers.Add(chatMember);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, RoomViewModel roomViewModel)
        {
            if (_context.Rooms.Any(r => r.Name == roomViewModel.Name))
                return BadRequest("Неверное название чата или чат уже существует");

            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            var chatMembers = await _context.ChatMembers.Where(r => r.SomeRoom == room).FirstOrDefaultAsync();
            if (room == null)
                return NotFound();
            if (!chatMembers.IsPrivate)
            {
                room.Name = roomViewModel.Name;
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("updateChatRoom", new { id = room.Id, room.Name });
            }
            
            return NoContent();
        }

        [HttpDelete]
        [Route("deleteUser")]
        public async Task<IActionResult> DeleteUserFromChat(RoomViewModel roomViewModel)
        {

            var room = await _context.Rooms.Where(r => r.Id == roomViewModel.Id).FirstOrDefaultAsync();
            if (room == null)
                return NotFound();
            var user = _context.Users.FirstOrDefault(u => u.UserName == roomViewModel.Name);
            if (user == null)
                return NotFound();
            if (user != room.Admin)
            {
                var chatMemeber = await _context.ChatMembers.Where(r => r.SomeRoom == room && r.SomeUser == user).FirstOrDefaultAsync();

                _context.ChatMembers.Remove(chatMemeber);
                await _context.SaveChangesAsync();
            }
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();
            var chatMemeber = await _context.ChatMembers.Where(r => r.SomeRoom == room).ToListAsync();
            for (int i = 0; i < chatMemeber.Count; i++)
            {
                _context.ChatMembers.Remove(chatMemeber[i]);
                await _context.SaveChangesAsync();
            }
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatRoom", room.Id);
            await _hubContext.Clients.Group(room.Name).SendAsync("onRoomDeleted", string.Format("Room {0} has been deleted.\nYou are moved to the first available room!", room.Name));

            return NoContent();
        }
    }
}
