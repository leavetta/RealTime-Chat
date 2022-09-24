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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            //var config = new MapperConfiguration(cfg => cfg.CreateMap<ApplicationUser, UserViewModel>());
            //_mapper = new Mapper(config);
        }

        /*public ApplicationDbContext GetContext()
        {
            return _context;
        }

        public IMapper GetMapper()
        {
            return _mapper;
        }*/

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> Get()
        {
            //var tempContext = GetContext();

            var users = await _context.AppUsers.ToListAsync();
            if (users == null)
            {
                Debug.Write("My error message. ");

                return BadRequest();
            }
            //Debug.Write("My error message. ");
            //var tempMapper = GetMapper();
            var usersViewModel = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UserViewModel>>(users);

            return Ok(usersViewModel);
        }

        [HttpGet("Room/{roomName}")]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetId(string roomName)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Name == roomName);
            Debug.WriteLine("THIS IS ROOOOOM-----------" + room.Name);
            if (room == null)
                return NotFound();
            var chatUsers = await _context.ChatMembers.Where(u => u.IsPrivate == false && u.SomeRoom == room)
                .Select(s => s.SomeUser)
                .ToListAsync();
            /*var users = chatMembers.Where(u => u.SomeRoom == room)
                .Select(s => s.SomeUser).ToList();

            foreach (var mem in chatMembers)
            {
                Debug.WriteLine(mem.Id);
            }*/

            if (chatUsers == null)
            {
                Debug.Write("My error message. ");

                return BadRequest();
            }
            //foreach (var user in chatUsers)
            //{
            //    Debug.WriteLine(user.FullName);
            //}

            //Debug.Write("My error message. ");
            var usersViewModel = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UserViewModel>>(chatUsers);

            return Ok(usersViewModel);
        }
    }
}
