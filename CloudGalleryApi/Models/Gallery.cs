namespace CloudGalleryApi.Models;

public class Gallery
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public string OwnerId { get; set; } // Az IdentityUser ID-ja
    public List<Photo> Photos { get; set; } = new();
    public List<GalleryShare> Shares { get; set; } = new();
}
