using Microsoft.AspNetCore.Mvc;

namespace SampleRestApi.Controllers
{
    [ApiController]
    [Route("ad-hoc")]
    public class AdhocController : ControllerBase
    {
        [HttpGet("headers-check")]
        public IActionResult MustHaveCertainHeaders()
        {
            if (Request.Headers["x-foo"] == "x-bar")
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
