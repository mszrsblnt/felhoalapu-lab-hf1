namespace CloudGalleryApi.Models;

public class PhotoUploadRequest
{
    public IFormFile File { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
}
