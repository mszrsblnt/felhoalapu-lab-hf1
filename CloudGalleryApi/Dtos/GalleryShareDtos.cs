namespace CloudGalleryApi.Dtos;

public record GalleryShareDto(int Id, string UserEmail, bool CanEdit);
public record GalleryShareDetailDto(int Id, int GalleryId, string UserEmail, bool CanEdit);
