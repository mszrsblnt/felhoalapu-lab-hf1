using CloudGalleryApi.Context;
using CloudGalleryApi.Dtos;
using CloudGalleryApi.Mappers;
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
            return await _db.Galleries.Include(g => g.Shares)
                .Where(g => g.IsPublic).ToListAsync();

        // Saját galériák
        if (mine && userId != null)
            return await _db.Galleries.Include(g => g.Shares)
                .Where(g => g.OwnerId == userId).ToListAsync();

        // Megosztott galériák
        if (sharedWithMe && userEmail != null)
        {
            var sharedIds = _db.GalleryShares.Where(s => s.UserEmail == userEmail).Select(s => s.GalleryId);
            return await _db.Galleries.Include(g => g.Shares)
                .Where(g => sharedIds.Contains(g.Id)).ToListAsync();
        }

        // Publikus galériák
        if (isPublic == true)
            return await _db.Galleries.Include(g => g.Shares)
                .Where(g => g.IsPublic).ToListAsync();

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
        return await _db.Galleries
            .Include(g => g.Photos)
            .Include(g => g.Shares)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Gallery> CreateGalleryAsync(CreateGalleryDto dto, string userId)
    {
        var gallery = dto.ToEntity();
        gallery.OwnerId = userId;
        gallery.CreatedAt = DateTime.UtcNow;
        _db.Galleries.Add(gallery);
        await _db.SaveChangesAsync();
        return gallery;
    }

    public async Task<bool> UpdateGalleryAsync(int id, UpdateGalleryDto dto, string userId)
    {
        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null || gallery.OwnerId != userId) return false;
        gallery.ApplyUpdate(dto);
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

    // ── Share management ──────────────────────────────────────────────────────

    public async Task<List<GalleryShare>?> GetSharesAsync(int galleryId, string userId)
    {
        var gallery = await _db.Galleries.FindAsync(galleryId);
        if (gallery == null || gallery.OwnerId != userId) return null;
        return await _db.GalleryShares.Where(s => s.GalleryId == galleryId).ToListAsync();
    }

    public async Task<GalleryShare?> AddShareAsync(int galleryId, GalleryShareDto dto, string userId)
    {
        var gallery = await _db.Galleries.FindAsync(galleryId);
        if (gallery == null || gallery.OwnerId != userId) return null;

        var existing = await _db.GalleryShares
            .FirstOrDefaultAsync(s => s.GalleryId == galleryId && s.UserEmail == dto.UserEmail);
        if (existing != null)
        {
            existing.CanEdit = dto.CanEdit;
            await _db.SaveChangesAsync();
            return existing;
        }

        var share = new GalleryShare { GalleryId = galleryId, UserEmail = dto.UserEmail, CanEdit = dto.CanEdit };
        _db.GalleryShares.Add(share);
        await _db.SaveChangesAsync();
        return share;
    }

    public async Task<bool> RemoveShareAsync(int shareId, string userId)
    {
        var share = await _db.GalleryShares
            .Include(s => s.Gallery)
            .FirstOrDefaultAsync(s => s.Id == shareId);
        if (share == null || share.Gallery.OwnerId != userId) return false;
        _db.GalleryShares.Remove(share);
        await _db.SaveChangesAsync();
        return true;
    }
}
