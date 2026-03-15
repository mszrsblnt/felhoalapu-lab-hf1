namespace CloudGalleryApi.Dtos;

public record PhotoDto(int Id, string Name, DateTime UploadedAt);
public record PhotoDetailDto(int Id, string Name, DateTime UploadedAt, int GalleryId, string ContentType);
