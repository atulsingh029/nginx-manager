using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nginx_manager.Models;

namespace nginx_manager.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HostController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NginxHost>>> Get()
        {
            var items = await _db.NginxHosts.OrderByDescending(h => h.Id).Take(100).ToListAsync();
            return Ok(items);
        }

        public class PingRequest
        {
            public string hostname { get; set; } = string.Empty;
            public string ip { get; set; } = string.Empty;
        }

        [HttpPost]
        [Route("ping")]
        public async Task<IActionResult> AgentPing([FromBody] PingRequest value)
        {
            if (string.IsNullOrWhiteSpace(value.hostname) || string.IsNullOrWhiteSpace(value.ip))
            {
                return BadRequest("hostname and ip are required");
            }

            var entity = new NginxHost
            {
                Hostname = value.hostname,
                Ip = value.ip,
                CreatedAt = DateTime.UtcNow
            };

            _db.NginxHosts.Add(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
