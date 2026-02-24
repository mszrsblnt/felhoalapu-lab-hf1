using CloudGalleryApi.Context;
using CloudGalleryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CloudGalleryApi.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GalleriesController(DataContext _db) : ControllerBase
{

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicGalleries()
    {
        var galleries = await _db.Galleries
            .Where(g => g.IsPublic)
            .ToListAsync();

        return Ok(galleries);
    }

    // 1. A bejelentkezett felhasználó saját galériái (Privát + Publikus is)
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyGalleries()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var galleries = await _db.Galleries
            .Where(g => g.OwnerId == userId)
            .ToListAsync();

        return Ok(galleries);
    }

    // 2. Galériák, amiket megosztottak az aktuális felhasználóval (Email alapján)
    [HttpGet("shared-with-me")]
    public async Task<IActionResult> GetSharedGalleries()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email); // Identity API beállítja a claimet
        if (string.IsNullOrEmpty(userEmail)) return BadRequest("Email claim nem található.");

        var sharedGalleries = await _db.GalleryShares
            .Where(s => s.UserEmail == userEmail)
            .Include(s => s.Gallery)
            .Select(s => new {
                s.Gallery,
                s.CanEdit // Visszaadjuk, hogy szerkesztheti-e
            })
            .ToListAsync();

        return Ok(sharedGalleries);
    }


    // 2. Galéria létrehozása (Csak bejelentkezve)
    [HttpPost]
    public async Task<IActionResult> CreateGallery([FromBody] Gallery gallery)
    {
        // Kiolvassuk a bejelentkezett user ID-ját a tokenből
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        gallery.OwnerId = userId;
        gallery.CreatedAt = DateTime.UtcNow;

        _db.Galleries.Add(gallery);
        await _db.SaveChangesAsync();

        return Ok(gallery);
    }

    // 3. Egy konkrét galéria lekérése
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGallery(int id)
    {
        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Ha nem publikus ÉS nem a tulajdonos kéri le, akkor tiltás
        if (!gallery.IsPublic && gallery.OwnerId != currentUserId) return Forbid();

        return Ok(gallery);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGallery(int id, [FromBody] Gallery updatedGallery)
    {
        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null) return NotFound();

        if (gallery.OwnerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

        gallery.Name = updatedGallery.Name;
        gallery.IsPublic = updatedGallery.IsPublic;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGallery(int id)
    {
        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null) return NotFound();

        if (gallery.OwnerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

        _db.Galleries.Remove(gallery);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}