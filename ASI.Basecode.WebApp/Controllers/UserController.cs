using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /users
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetUsers()
        {
            // Return minimal user info without password
            List<object> users = new List<object>();
            if (_userService != null)
            {
                users = _userService
                        .GetAllUsers()
                        .Select(u => new { id = u.Id, userId = u.UserId, name = u.Name, email = u.Email, role = u.Role, status = u.Status })
                        .Cast<object>()
                        .ToList();
            }

            return Ok(users);
        }

        // POST /users
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Create([FromBody] FrontendUserDto model)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.name) || string.IsNullOrWhiteSpace(model.email) || string.IsNullOrWhiteSpace(model.password))
                return BadRequest(new { message = "Missing required fields" });

            var user = new User
            {
                // Use username as UserId so it maps to existing services
                UserId = model.name,
                Name = model.name,
                Email = model.email,
                Password = model.password,
                Role = string.IsNullOrWhiteSpace(model.role) ? "User" : model.role,
                Status = string.IsNullOrWhiteSpace(model.status) ? "Active" : model.status
            };

            var result = _userService.RegisterUser(user);
            if (result == ASI.Basecode.Resources.Constants.Enums.LoginResult.Success)
            {
                return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { success = true });
            }

            return Conflict(new { success = false, message = "Registration failed or user exists" });
        }

        // DTO matching the React app payload shape
        public class FrontendUserDto
        {
            public string name { get; set; }
            public string email { get; set; }
            public string password { get; set; }
            public string role { get; set; }
            public string id { get; set; }
            public string status { get; set; }
        }
    }
}
