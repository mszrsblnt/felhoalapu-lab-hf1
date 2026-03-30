using CloudGalleryApi.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloudGalleryApi.Context;

public class DataContext : IdentityDbContext<IdentityUser>, IDataProtectionKeyContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public DbSet<Photo> Photos { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<GalleryShare> GalleryShares { get; set; }
}
