using CloudGalleryApi.Context;
using CloudGalleryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudGalleryApi.Services;

public class GalleryService
{
    private readonly DataContext _db;
    public GalleryService(DataContext db)
    {
        _db = db;
    }

    public async Task<List<Gallery>> GetGalleriesAsync(string? userId, string? userEmail, bool? isPublic, bool mine, bool sharedWithMe)
    {
        // Bejelentkezetlen felhasználó, csak publikus galériák
        if (userId == null && userEmail == null)
            return await _db.Galleries.Where(g => g.IsPublic).ToListAsync();

        // Saját galériák
        if (mine && userId != null)
            return await _db.Galleries.Where(g => g.OwnerId == userId).ToListAsync();

        // Megosztott galériák
        if (sharedWithMe && userEmail != null)
            return await _db.GalleryShares.Where(s => s.UserEmail == userEmail).Select(s => s.Gallery).ToListAsync();

        // Publikus galériák (bejelentkezett felhasználónak is)
        if (isPublic == true)
            return await _db.Galleries.Where(g => g.IsPublic).ToListAsync();

        return await _db.Galleries
            .Include(g => g.Shares)
            .Where(g =>
                g.IsPublic
                || (userId != null && g.OwnerId == userId)
                || (userEmail != null && g.Shares.Any(s => s.UserEmail == userEmail))
            )
            .ToListAsync();
    }

    public async Task<Gallery?> GetGalleryAsync(int id)
    {
        return await _db.Galleries.Include(g => g.Shares).FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Gallery> CreateGalleryAsync(Gallery gallery, string userId)
    {
        gallery.OwnerId = userId;
        gallery.CreatedAt = DateTime.UtcNow;
        _db.Galleries.Add(gallery);
        await _db.SaveChangesAsync();
        return gallery;
    }

    public async Task<bool> UpdateGalleryAsync(int id, Gallery updatedGallery, string userId)
    {
        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null || gallery.OwnerId != userId) return false;
        gallery.Name = updatedGallery.Name;
        gallery.IsPublic = updatedGallery.IsPublic;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGalleryAsync(int id, string userId)
    {
        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null || gallery.OwnerId != userId) return false;
        _db.Galleries.Remove(gallery);
        await _db.SaveChangesAsync();
        return true;
    }
}
