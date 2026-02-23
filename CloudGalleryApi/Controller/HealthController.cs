using Microsoft.AspNetCore.Mvc;

namespace CloudGalleryApi.Controller;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        return "Your backend is working :)";
    }
}
