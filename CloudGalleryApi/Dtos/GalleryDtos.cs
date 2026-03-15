namespace CloudGalleryApi.Dtos;

public record GalleryDto(
    int Id,
    string Name,
    DateTime CreatedAt,
    bool IsPublic,
    string OwnerId,
    bool IsOwner,
    bool SharedWithMe,
    bool SharedCanEdit
);

public record GalleryDetailDto(
    int Id,
    string Name,
    DateTime CreatedAt,
    bool IsPublic,
    string OwnerId,
    bool IsOwner,
    IReadOnlyList<PhotoDto> Photos,
    IReadOnlyList<GalleryShareDto> Shares
);
public record CreateGalleryDto(string Name, bool IsPublic, string? CoverUrl = null);

public record UpdateGalleryDto(string Name, bool IsPublic);
