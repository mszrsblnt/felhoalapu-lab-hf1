using CloudGalleryApi.Dtos;
using CloudGalleryApi.Models;

namespace CloudGalleryApi.Mappers;

public static class GalleryMapper
{
    public static GalleryDto ToDto(this Gallery g, string? currentUserId, string? currentUserEmail)
    {
        var share = g.Shares?.FirstOrDefault(s => s.UserEmail == currentUserEmail);
        var isOwner = g.OwnerId == currentUserId;
        return new(g.Id, g.Name, g.CreatedAt, g.IsPublic, g.OwnerId,
            IsOwner: isOwner,
            SharedWithMe: !isOwner && share != null,
            SharedCanEdit: !isOwner && share?.CanEdit == true);
    }

    public static IReadOnlyList<GalleryDto> ToDto(this IEnumerable<Gallery> galleries, string? currentUserId, string? currentUserEmail) =>
        galleries.Select(g => g.ToDto(currentUserId, currentUserEmail)).ToList();

    public static GalleryDetailDto ToDetailDto(this Gallery g, string? currentUserId) =>
        new(
            g.Id,
            g.Name,
            g.CreatedAt,
            g.IsPublic,
            g.OwnerId,
            IsOwner: g.OwnerId == currentUserId,
            Photos: g.Photos.ToDto(),
            Shares: g.Shares.ToDto()
        );

    public static IReadOnlyList<GalleryDetailDto> ToDetailDto(this IEnumerable<Gallery> galleries, string? currentUserId) =>
        galleries.Select(g => g.ToDetailDto(currentUserId)).ToList();

    public static Gallery ToEntity(this CreateGalleryDto dto) =>
        new() { Name = dto.Name, IsPublic = dto.IsPublic  };

    public static void ApplyUpdate(this Gallery g, UpdateGalleryDto dto)
    {
        g.Name = dto.Name;
        g.IsPublic = dto.IsPublic;
    }
}
