using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace StorkItmeServer.Controllers
{
    [ApiController]
    [Route("[controller]"), Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        [Authorize(Policy = "Manager")]
        public IActionResult Get()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var userName = User.Identity?.Name;

                _logger.LogInformation($"User {userName} (ID: {userId}) is accessing weather forecast.");

                var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                    .ToArray();

                return Ok(forecasts);
            }
            else
                return Unauthorized("User is not authenticated.");

        }
    }
}
