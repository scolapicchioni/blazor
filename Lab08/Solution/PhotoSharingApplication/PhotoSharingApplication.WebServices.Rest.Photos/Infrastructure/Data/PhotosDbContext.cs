using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;

public class PhotosDbContext : DbContext {
    public PhotosDbContext(DbContextOptions<PhotosDbContext> options): base(options) {    }
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
