using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace southafricantaxtool.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController : ControllerBase
    {
        private readonly ILogger<TaxController> _logger;

        public TaxController(ILogger<TaxController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogDebug("Hit endpoint");
            await Task.Delay(500);
            return Ok("success");
        }
    }
}
