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

    private bool CanAccessGallery(Gallery gallery, string? userId, string? userEmail)
        => gallery.IsPublic
        || (userId != null && gallery.OwnerId == userId)
        || (userEmail != null && gallery.Shares.Any(s => s.UserEmail == userEmail));

    public async Task<List<Photo>> GetPhotosAsync(
        int galleryId, string? userId, string? userEmail,
        string sortBy = "date", bool sortDesc = true)
    {
        var gallery = await _db.Galleries.Include(g => g.Shares)
            .FirstOrDefaultAsync(g => g.Id == galleryId);
        if (gallery == null || !CanAccessGallery(gallery, userId, userEmail))
            return new List<Photo>();

        IQueryable<Photo> query = _db.Photos.Where(p => p.GalleryId == galleryId);

        query = (sortBy.ToLower(), sortDesc) switch
        {
            ("name", true) => query.OrderByDescending(p => p.Name),
            ("name", false) => query.OrderBy(p => p.Name),
            (_, true) => query.OrderByDescending(p => p.UploadedAt),
            (_, false) => query.OrderBy(p => p.UploadedAt),
        };

        return await query.ToListAsync();
    }

    public async Task<Photo?> GetPhotoAsync(int id, string? userId, string? userEmail)
    {
        var photo = await _db.Photos
            .Include(p => p.Gallery).ThenInclude(g => g.Shares)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null) return null;
        return CanAccessGallery(photo.Gallery, userId, userEmail) ? photo : null;
    }

    public async Task<Photo?> UploadPhotoAsync(int galleryId, Photo photo, string userId, string? userEmail)
    {
        var gallery = await _db.Galleries.Include(g => g.Shares)
            .FirstOrDefaultAsync(g => g.Id == galleryId);
        if (gallery == null) return null;
        if (gallery.OwnerId != userId && !(userEmail != null && gallery.Shares.Any(s => s.UserEmail == userEmail && s.CanEdit)))
            return null;
        photo.GalleryId = galleryId;
        photo.UploadedAt = DateTime.UtcNow;
        _db.Photos.Add(photo);
        await _db.SaveChangesAsync();
        return photo;
    }

    public async Task<bool> DeletePhotoAsync(int id, string userId, string? userEmail)
    {
        var photo = await _db.Photos.Include(p => p.Gallery)
            .ThenInclude(g => g.Shares)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (photo == null) 
            return false;
        if (photo.Gallery.OwnerId != userId && !(userEmail != null && photo.Gallery.Shares.Any(s => s.UserEmail == userEmail && s.CanEdit))) 
            return false;
        
        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Photo?> GetCoverPhotoAsync(int galleryId, string? userId, string? userEmail)
    {
        var gallery = await _db.Galleries.Include(g => g.Shares)
            .FirstOrDefaultAsync(g => g.Id == galleryId);
        if (gallery == null || !CanAccessGallery(gallery, userId, userEmail))
            return null;

        return await _db.Photos
            .Where(p => p.GalleryId == galleryId)
            .OrderBy(p => p.UploadedAt)
            .FirstOrDefaultAsync();
    }
}
