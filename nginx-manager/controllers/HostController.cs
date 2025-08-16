using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace nginx_manager.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostController : ControllerBase
    {
        private readonly IHostEnvironment _environment;
        public HostController(IHostEnvironment environment)
        { }
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "Hello World!";
        }       
    }
}
