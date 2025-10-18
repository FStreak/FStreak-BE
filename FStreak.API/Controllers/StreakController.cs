using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FStreak.Domain.Entities;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;

namespace FStreak.API.Controllers
{
    // This controller is deprecated. Please use StreaksController instead.
    // Will be removed in a future update.
    [ApiController]
    [Route("api/streak")]
    [Obsolete("This controller is deprecated. Please use StreaksController instead.")]
    public class StreakController : ControllerBase
    {
        private readonly IStreakService _streakService;

        public StreakController(IStreakService streakService)
        {
            _streakService = streakService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("/api/streaks");
        }
    }
}