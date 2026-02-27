using CloudGalleryApi.Models;
using CloudGalleryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudGalleryApi.Controller;


[Authorize]
[ApiController]
 [Route("api/photos")]
public class PhotosController(PhotoService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhotos([FromQuery] int galleryId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var photos = await service.GetPhotosAsync(galleryId, userId, userEmail);
        // Csak publikus vagy jogosult galéria képeit adja vissza
        return Ok(photos.Select(p => new { p.Id, p.Name, p.UploadedAt }));
    }

    [HttpGet("{id}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage([FromQuery] int galleryId, [FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var photo = await service.GetPhotoAsync(id, userId, userEmail);
        if (photo == null) return Forbid();
        return File(photo.ImageData, photo.ContentType);
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto([FromQuery] int galleryId, [FromForm] PhotoUploadRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        using var ms = new MemoryStream();
        await request.File.CopyToAsync(ms);

        var photo = new Photo
        {
            Name = request.Name.Length > 40 ? request.Name[..40] : request.Name,
            ImageData = ms.ToArray(),
            ContentType = request.File.ContentType,
        };

        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var created = await service.UploadPhotoAsync(galleryId, photo, userId, userEmail);
        if (created == null) return Forbid();
        return Ok(new { created.Id, created.Name });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto([FromQuery] int galleryId, [FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var ok = await service.DeletePhotoAsync(id, userId);
        return ok ? NoContent() : Forbid();
    }
}
