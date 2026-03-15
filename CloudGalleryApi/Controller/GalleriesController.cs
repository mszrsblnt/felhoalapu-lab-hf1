using CloudGalleryApi.Dtos;
using CloudGalleryApi.Mappers;
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
        return Ok(galleries.ToDto(userId, userEmail));
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
            if (gallery.OwnerId != currentUserId && !gallery.Shares.Any(s => s.UserEmail == currentUserEmail))
                return Forbid();
        }
        return Ok(gallery.ToDetailDto(currentUserId));
    }

    [HttpPost]
    public async Task<IActionResult> CreateGallery([FromBody] CreateGalleryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var created = await service.CreateGalleryAsync(dto, userId);
        return Ok(created.ToDetailDto(userId));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGallery([FromRoute] int id, [FromBody] UpdateGalleryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var ok = await service.UpdateGalleryAsync(id, dto, userId);
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

    // ── Share endpoints ────────────────────────────────────────────────────────

    [HttpGet("{id}/shares")]
    public async Task<IActionResult> GetShares([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var shares = await service.GetSharesAsync(id, userId);
        if (shares == null) return Forbid();
        return Ok(shares.ToDetailDto());
    }

    [HttpPost("{id}/shares")]
    public async Task<IActionResult> AddShare([FromRoute] int id, [FromBody] GalleryShareDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var share = await service.AddShareAsync(id, dto, userId);
        if (share == null) return Forbid();
        return Ok(share.ToDetailDto());
    }

    [HttpDelete("{id}/shares/{shareId}")]
    public async Task<IActionResult> RemoveShare([FromRoute] int id, [FromRoute] int shareId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var ok = await service.RemoveShareAsync(shareId, userId);
        return ok ? NoContent() : Forbid();
    }
}
