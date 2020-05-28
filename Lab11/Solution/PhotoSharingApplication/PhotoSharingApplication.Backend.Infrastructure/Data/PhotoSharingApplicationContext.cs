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
            modelBuilder.Entity<Comment>(ConfigureComment);
        }

        private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
            builder.ToTable("Photos");

            builder.Property(ci => ci.Id)
                .UseHiLo("photos_hilo")
                .IsRequired();

            builder.Property(ci => ci.Title)
                .IsRequired(true)
                .HasMaxLength(255);
        }

        private void ConfigureComment(EntityTypeBuilder<Comment> builder) {
            builder.ToTable("Comments");

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Id)
                .UseHiLo("comments_hilo")
               .IsRequired();

            builder.Property(cb => cb.Subject)
                .IsRequired()
                .HasMaxLength(250);

            builder.HasOne(ci => ci.Photo)
                .WithMany(bz => bz.Comments)
                .HasForeignKey(ci => ci.PhotoId);

            builder.Property(c => c.PhotoId).IsRequired();
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
