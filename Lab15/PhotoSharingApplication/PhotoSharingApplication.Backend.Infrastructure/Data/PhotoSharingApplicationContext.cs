using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Core.Entities;
using System;

namespace PhotoSharingApplication.Backend.Infrastructure.Data {
    public class PhotoSharingApplicationContext : DbContext {
        public PhotoSharingApplicationContext(DbContextOptions<PhotoSharingApplicationContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.LogTo(Console.WriteLine);
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Photo>(ConfigurePhoto);
            modelBuilder.Entity<Comment>(ConfigureComment);
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

            builder.Property(p => p.PhotoFile)
                .IsRequired(true);

            builder.Property(p => p.ImageMimeType)
                .IsRequired(true)
                .HasMaxLength(255);
        }
        private void ConfigureComment(EntityTypeBuilder<Comment> builder) {
            builder.ToTable("Comments");

            builder.Property(comment => comment.Subject)
                .IsRequired()
                .HasMaxLength(250);
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoImage> PhotoImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
