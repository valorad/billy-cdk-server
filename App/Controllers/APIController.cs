using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class APIController : ControllerBase
    {
        [HttpGet]
        public JsonResult ReadyState()
        {
            string message = "{\"ok\": true, \"message\": \"API works!\" }";
            return new JsonResult(JsonDocument.Parse(message).RootElement);
        }
    }

}