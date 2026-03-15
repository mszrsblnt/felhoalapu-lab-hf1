using CloudGalleryApi.Dtos;
using CloudGalleryApi.Models;

namespace CloudGalleryApi.Mappers;
public static class PhotoMapper
{
    public static PhotoDto ToDto(this Photo p) =>
        new(p.Id, p.Name, p.UploadedAt);

    public static IReadOnlyList<PhotoDto> ToDto(this IEnumerable<Photo> photos) =>
        photos.Select(p => p.ToDto()).ToList();

    public static PhotoDetailDto ToDetailDto(this Photo p) =>
        new(p.Id, p.Name, p.UploadedAt, p.GalleryId, p.ContentType);

    public static IReadOnlyList<PhotoDetailDto> ToDetailDto(this IEnumerable<Photo> photos) =>
        photos.Select(p => p.ToDetailDto()).ToList();
}
