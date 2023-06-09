using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.WebServices.REST.Photos.Infrastructure.Data {
    public class PhotosDbContext : DbContext {
        public PhotosDbContext(DbContextOptions<PhotosDbContext> options) : base(options) {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Photo>(ConfigurePhoto);
            modelBuilder.Entity<PhotoImage>(ConfigurePhotoImage);
        }
        private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
            builder.ToTable("Photos");

            builder.Property(photo => photo.Title)
                .IsRequired(true)
                .HasMaxLength(255);

            builder.Ignore(p => p.ImageUrl);

            builder.HasOne(p => p.PhotoImage)
                .WithOne()
                .HasForeignKey<PhotoImage>(p => p.Id);
        }
        private void ConfigurePhotoImage(EntityTypeBuilder<PhotoImage> builder) {
            builder.ToTable("Photos");
        }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoImage> PhotoImages { get; set; }
    }
}
