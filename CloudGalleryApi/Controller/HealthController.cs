using CloudGalleryApi.Context;
using Microsoft.AspNetCore.Mvc;

namespace CloudGalleryApi.Controller;

[ApiController]
[Route("[controller]")]
public class HealthController(DataContext _db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        bool isDbConnected = await _db.Database.CanConnectAsync();

        return Ok(new
        {
            Backend = "Healthy",
            Database = isDbConnected ? "Connected" : "Disconnected",
            TimeStamp = DateTime.Now.ToLocalTime(),
        });
    }
}