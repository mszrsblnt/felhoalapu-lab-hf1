namespace CloudGalleryApi.Models;

public class GalleryShare
{
    public int Id { get; set; }
    public int GalleryId { get; set; }
    public Gallery Gallery { get; set; }
    public bool CanEdit { get; set; }
    public string UserEmail { get; set; }
}
