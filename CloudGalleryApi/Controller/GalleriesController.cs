using CloudGalleryApi.Models;
using CloudGalleryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudGalleryApi.Controller;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GalleriesController(GalleryService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetGalleries([FromQuery] bool? isPublic, [FromQuery] bool? mine, [FromQuery] bool? sharedWithMe)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var galleries = await service.GetGalleriesAsync(userId, userEmail, isPublic, mine ?? false, sharedWithMe ?? false);
        return Ok(galleries);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGallery([FromRoute] int id)
    {
        var gallery = await service.GetGalleryAsync(id);
        if (gallery == null) return NotFound();
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
        if (!gallery.IsPublic)
        {
            if (currentUserId == null) return Forbid();
            if (gallery.OwnerId != currentUserId && (gallery.Shares == null || !gallery.Shares.Any(s => s.UserEmail == currentUserEmail)))
                return Forbid();
        }
        return Ok(gallery);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGallery([FromBody] Gallery gallery)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var created = await service.CreateGalleryAsync(gallery, userId);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGallery([FromRoute] int id, [FromBody] Gallery updatedGallery)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var ok = await service.UpdateGalleryAsync(id, updatedGallery, userId);
        return ok ? NoContent() : Forbid();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGallery([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var ok = await service.DeleteGalleryAsync(id, userId);
        return ok ? NoContent() : Forbid();
    }
}
