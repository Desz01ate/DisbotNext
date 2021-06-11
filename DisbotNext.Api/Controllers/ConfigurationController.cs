using DisbotNext.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Api.Controllers
{
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly DiscordSettings configuration;
        private readonly IHostApplicationLifetime appLifeTime;

        public ConfigurationController(
            IHostApplicationLifetime appLifeTime,
            IOptionsSnapshot<DiscordSettings> configuration)
        {
            this.configuration = configuration.Value;
            this.appLifeTime = appLifeTime;
        }

        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult GetConfiguration()
        {
            return this.Ok(this.configuration);
        }

        [HttpGet]
        [Route("api/[controller]/{key}")]
        public IActionResult GetSpecificConfig(string key)
        {
            var property = typeof(DiscordSettings).GetProperty(key);
            if (property == null)
                return NotFound();

            var value = property.GetValue(this.configuration);
            return this.Ok(value);
        }

        [HttpPut]
        [Route("api/[controller]/{key}/{value}")]
        public IActionResult UpdateSpecificConfg(string key, string value)
        {
            var property = typeof(DiscordSettings).GetProperty(key);
            if (property == null)
                return NotFound();

            ConfigurationHelpers.AddOrUpdateAppSetting($"Discord:{key}", value);
            return this.StatusCode(204);
        }

        [HttpPost]
        [Route("api/[controller]/restart")]
        public IActionResult RestartService()
        {
            //TODO: Find a way to restart application in self-hosted environment.
            //appLifeTime.StopApplication();
            return this.Ok(true);
        }
    }
}
