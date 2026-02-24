namespace CloudGalleryApi.Models;

public class Photo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public byte[] ImageData { get; set; } 
    public string ContentType { get; set; } 
    public DateTime UploadedAt { get; set; }
    public int GalleryId { get; set; }
    public Gallery Gallery { get; set; }
}
