using CloudGalleryApi.Context;
using CloudGalleryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudGalleryApi.Services;

public class PhotoService
{
    private readonly DataContext _db;
    public PhotoService(DataContext db)
    {
        _db = db;
    }

    public async Task<List<Photo>> GetPhotosAsync(int galleryId, string? userId, string? userEmail)
    {
        var gallery = await _db.Galleries.Include(g => g.Shares).FirstOrDefaultAsync(g => g.Id == galleryId);
        if (gallery == null)
            return new List<Photo>();

        // Publikus galéria, bárki láthatja
        if (gallery.IsPublic)
            return await _db.Photos.Where(p => p.GalleryId == galleryId).ToListAsync();

        // Bejelentkezett felhasználó, csak saját vagy megosztott galéria
        if (userId != null)
        {
            if (gallery.OwnerId == userId)
                return await _db.Photos.Where(p => p.GalleryId == galleryId).ToListAsync();
            if (gallery.Shares.Any(s => s.UserEmail == userEmail))
                return await _db.Photos.Where(p => p.GalleryId == galleryId).ToListAsync();
        }

        return new List<Photo>();
    }

    public async Task<Photo?> GetPhotoAsync(int id, string? userId, string? userEmail)
    {
        var photo = await _db.Photos.Include(p => p.Gallery).ThenInclude(g => g.Shares).FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null)
            return null;
        var gallery = photo.Gallery;
        if (gallery.IsPublic)
            return photo;
        if (userId != null)
        {
            if (gallery.OwnerId == userId)
                return photo;
            if (gallery.Shares.Any(s => s.UserEmail == userEmail))
                return photo;
        }
        return null;
    }

    public async Task<Photo?> UploadPhotoAsync(int galleryId, Photo photo, string userId, string? userEmail)
    {
        var gallery = await _db.Galleries.Include(g => g.Shares).FirstOrDefaultAsync(g => g.Id == galleryId);
        if (gallery == null)
            return null;
        if (gallery.OwnerId != userId && !(userEmail != null && gallery.Shares.Any(s => s.UserEmail == userEmail && s.CanEdit)))
            return null;
        photo.GalleryId = galleryId;
        photo.UploadedAt = DateTime.UtcNow;
        _db.Photos.Add(photo);
        await _db.SaveChangesAsync();
        return photo;
    }

    public async Task<bool> DeletePhotoAsync(int id, string userId)
    {
        var photo = await _db.Photos.Include(p => p.Gallery).FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null)
            return false;
        if (photo.Gallery.OwnerId != userId)
            return false;
        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync();
        return true;
    }
}
