using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Server.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IMongoDatabase _database;

        public HealthController(IMongoDatabase database)
        {
            _database = database;
        }

        [HttpGet("mongo")]
        public IActionResult MongoHealth()
        {
            try
            {
                _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();
                return Ok("MongoDB is reachable");
            }
            catch
            {
                return StatusCode(500, "MongoDB unreachable");
            }
        }
    }
}
