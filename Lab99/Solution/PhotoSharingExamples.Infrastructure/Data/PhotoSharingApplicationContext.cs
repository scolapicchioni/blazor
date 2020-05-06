using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingExamples.Shared.Entities;

namespace PhotoSharingExamples.Backend.Infrastructure.Data
{
    public class PhotoSharingApplicationContext : DbContext
    {
        public PhotoSharingApplicationContext(DbContextOptions<PhotoSharingApplicationContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Photo>(ConfigurePhoto);
            modelBuilder.Entity<Comment>(ConfigureComment);

        }

        private void ConfigureComment(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Id)
                //.UseIdentityColumn()
                .UseHiLo("comments_hilo")
               //.ForSqlServerUseSequenceHiLo("comments_hilo")
               .IsRequired();

            builder.Property(cb => cb.Subject)
                .IsRequired()
                .HasMaxLength(250);

            builder.HasOne(ci => ci.Photo)
                .WithMany(bz => bz.Comments)
                .HasForeignKey(ci => ci.PhotoId);

            builder.Property(c => c.PhotoId).IsRequired();
        }

        private void ConfigurePhoto(EntityTypeBuilder<Photo> builder)
        {
            builder.ToTable("Photos");

            builder.Property(ci => ci.Id)
                //.UseIdentityColumn()
                .UseHiLo("photos_hilo")
                 //.ForSqlServerUseSequenceHiLo("photos_hilo")
                 .IsRequired();

            builder.Property(ci => ci.Title)
                .IsRequired(true)
                .HasMaxLength(255);

        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
