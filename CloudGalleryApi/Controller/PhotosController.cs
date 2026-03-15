using CloudGalleryApi.Mappers;
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
    public async Task<IActionResult> GetPhotos([FromQuery] int galleryId, [FromQuery] string? sortBy, [FromQuery] bool? sortDesc)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var photos = await service.GetPhotosAsync(galleryId, userId, userEmail, sortBy ?? "date", sortDesc ?? true);
        return Ok(photos.ToDto());
    }

    [HttpGet("cover")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCoverImage([FromQuery] int galleryId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var photo = await service.GetCoverPhotoAsync(galleryId, userId, userEmail);
        if (photo == null) return NotFound();
        return File(photo.ImageData, photo.ContentType);
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
        return Ok(created.ToDto());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto([FromQuery] int galleryId, [FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var ok = await service.DeletePhotoAsync(id, userId, userEmail);
        return ok ? NoContent() : Forbid();
    }
}
