using CloudGalleryApi.Context;
using CloudGalleryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CloudGalleryApi.Controller;

[Authorize]
[ApiController]
[Route("api/galleries/{galleryId}/[controller]")]
public class PhotosController(DataContext _db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhotos(int galleryId)
    {
        var photos = await _db.Photos
            .Where(p => p.GalleryId == galleryId)
            .Select(p => new { p.Id, p.Name, p.UploadedAt })
            .ToListAsync();
        return Ok(photos);
    }

    [HttpGet("{id}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(int id)
    {
        var photo = await _db.Photos.FindAsync(id);
        if (photo == null) return NotFound();
        return File(photo.ImageData, photo.ContentType);
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(int galleryId, [FromForm] IFormFile file, [FromForm] string name)
    {
        var gallery = await _db.Galleries.FindAsync(galleryId);
        if (gallery == null || gallery.OwnerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var photo = new Photo
        {
            GalleryId = galleryId,
            Name = name.Length > 40 ? name[..40] : name,
            ImageData = ms.ToArray(),
            ContentType = file.ContentType,
            UploadedAt = DateTime.UtcNow
        };

        _db.Photos.Add(photo);
        await _db.SaveChangesAsync();
        return Ok(new { photo.Id, photo.Name });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(int galleryId, int id)
    {
        var photo = await _db.Photos.Include(p => p.Gallery).FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null || photo.Gallery.OwnerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}