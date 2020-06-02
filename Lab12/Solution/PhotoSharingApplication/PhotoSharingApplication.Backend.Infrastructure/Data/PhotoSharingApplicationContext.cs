using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Backend.Infrastructure.Data {
    public class PhotoSharingApplicationContext : DbContext {
        public PhotoSharingApplicationContext(DbContextOptions<PhotoSharingApplicationContext> options)
            : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Photo>(ConfigurePhoto);
            modelBuilder.Entity<PhotoImage>(ConfigurePhotoImage);
            modelBuilder.Entity<Comment>(ConfigureComment);
        }

        private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
            builder.ToTable("Photos");

            builder.Property(p => p.Id)
                .UseHiLo("photos_hilo")
                .IsRequired();

            builder.Property(p => p.Title)
                .IsRequired(true)
                .HasMaxLength(255);

            builder.Ignore(p=>p.ImageUrl);

            builder.HasOne(p => p.PhotoImage)
                .WithOne()
                .HasForeignKey<PhotoImage>(p => p.Id);
        }
        private void ConfigurePhotoImage(EntityTypeBuilder<PhotoImage> builder) {
            builder.ToTable("Photos");

            builder.Property(p => p.Id)
                .UseHiLo("photos_hilo")
                .IsRequired();

            builder.Property(p => p.PhotoFile)
                .IsRequired(true);

            builder.Property(p => p.ImageMimeType)
                .IsRequired(true)
                .HasMaxLength(255);
        }

        private void ConfigureComment(EntityTypeBuilder<Comment> builder) {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .UseHiLo("comments_hilo")
               .IsRequired();

            builder.Property(c => c.Subject)
                .IsRequired()
                .HasMaxLength(250);

            builder.HasOne(c => c.Photo)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PhotoId);

            builder.Property(c => c.PhotoId).IsRequired();
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoImage> PhotoImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
