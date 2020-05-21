using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {

        private readonly IPlayerService playerService;

        public PlayerController(IPlayerService playerService)
        {
            this.playerService = playerService;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<Player>> GetAll()
        {
            IEnumerable<Player> result = await playerService.GetList(JsonDocument.Parse("{}").RootElement);
            return result;
        }
    }

}