using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Backend.Infrastructure.Data;

public class PhotoSharingApplicationContext : DbContext {
    public PhotoSharingApplicationContext(DbContextOptions<PhotoSharingApplicationContext> options)
  : base(options) {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Photo>(ConfigurePhoto);
    }

    private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
        builder.ToTable("Photos");

        builder.Property(photo => photo.Title)
            .IsRequired(true)
            .HasMaxLength(255);
    }
    public DbSet<Photo> Photos { get; set; }
}
