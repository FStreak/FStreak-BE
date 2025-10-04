using FStreak.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
    }
}
