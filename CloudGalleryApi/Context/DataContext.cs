using CloudGalleryApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloudGalleryApi.Context;

public class DataContext : IdentityDbContext<IdentityUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }


    // Ide jönnek a tábláid, pl: 
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<GalleryShare> GalleryShares { get; set; }
}
