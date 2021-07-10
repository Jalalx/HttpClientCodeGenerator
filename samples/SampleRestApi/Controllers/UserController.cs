using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleRestApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SampleRestApi.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        private static readonly List<User> _users = new List<User>
        {
            new User { FirstName = "Will", LastName = "Smith", PhoneNumber = "+981234567" },
            new User { FirstName = "John", LastName = "Doe", PhoneNumber = "+987654321" },
            new User { FirstName = "Carl", LastName = "Hox", PhoneNumber = "+987987721" }
        };

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id > _users.Count || id <= 0)
            {
                return NotFound();
            }

            return Ok(_users[id - 1]);
        }

        [HttpGet("wrapped/{id}")]
        public IActionResult GetWrapped(int id)
        {
            if (id > _users.Count || id <= 0)
            {
                return NotFound();
            }

            return Ok(new HttpResultDto<User>(_users[id - 1]));
        }

        [HttpGet("search")]
        public IActionResult SearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            var result = _users.Where(x => x.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        x.LastName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToArray();
            return Ok(result);
        }

        [HttpGet("wrapped/search")]
        public IActionResult WrappedSearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            var result = _users.Where(x => x.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        x.LastName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToArray();
            return Ok(new HttpResultDto<User[]>(result));
        }

        [HttpPost("")]
        public IActionResult CreateUser(Models.User request)
        {
            _users.Add(request);
            return Ok(request);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, Models.User request)
        {
            if (id > _users.Count || id <= 0)
            {
                return NotFound();
            }

            var user = _users[id - 1];
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            return Ok(request);
        }

        [HttpDelete("{id}")]
        public IActionResult RemoveUser(int id)
        {
            if (id > _users.Count || id <= 0)
            {
                return NotFound();
            }

            _users.RemoveAt(id - 1);
            return Ok();
        }
    }
}
