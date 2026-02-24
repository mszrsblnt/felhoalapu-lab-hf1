using CloudGalleryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudGalleryApi.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }


    // Ide jönnek a tábláid, pl: 
    public DbSet<Photo> Photos { get; set; }
}
