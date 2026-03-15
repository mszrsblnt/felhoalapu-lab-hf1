using CloudGalleryApi.Dtos;
using CloudGalleryApi.Models;

namespace CloudGalleryApi.Mappers;
public static class GalleryShareMapper
{
    public static GalleryShareDto ToDto(this GalleryShare s) =>
        new(s.Id, s.UserEmail, s.CanEdit);

    public static IReadOnlyList<GalleryShareDto> ToDto(this IEnumerable<GalleryShare> shares) =>
        shares.Select(s => s.ToDto()).ToList();

    public static GalleryShareDetailDto ToDetailDto(this GalleryShare s) =>
        new(s.Id, s.GalleryId, s.UserEmail, s.CanEdit);

    public static IReadOnlyList<GalleryShareDetailDto> ToDetailDto(this IEnumerable<GalleryShare> shares) =>
        shares.Select(s => s.ToDetailDto()).ToList();
}
