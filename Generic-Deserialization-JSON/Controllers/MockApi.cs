#region --References--
using Generic_Deserialization_JSON.Core.Interfaces;
using Generic_Deserialization_JSON.Models;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace Generic_Deserialization_JSON.Controllers
{
    [ApiController]
    [Route("MockAPI")]
    public class MockApi : ControllerBase
    {
        private readonly ILogger<MockApi> logger;

        private readonly IObjectMapper<MockModel> _objectMapper;

        public MockApi(ILogger<MockApi> logger, IObjectMapper<MockModel> objectMapper)
        {
            this.logger = logger;
            _objectMapper = objectMapper;
        }

        [HttpPost(Name = "MapJsonToClass")]
        public async Task<IActionResult> Get()
        {
            using StreamReader reader = new(Request.Body);
            string requestBody = await reader.ReadToEndAsync();

            //Need to access configuration from hosted data source
            var filePath = Path.Combine("Mapping Config", "MockMapping.json");

            if (!System.IO.File.Exists(filePath))
            {
                return BadRequest("Config File Does not Exists");
            }

            string configText = System.IO.File.ReadAllText(filePath);
            try
            {
                var result = _objectMapper.MapToSingle(requestBody, configText);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
