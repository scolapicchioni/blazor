using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Data; 
public class CommentsDbContext : DbContext {
    public CommentsDbContext(DbContextOptions<CommentsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Comment>(ConfigureComments);
    }

    private void ConfigureComments(EntityTypeBuilder<Comment> builder) {
        builder.ToTable("Comments");

        builder.Property(comment => comment.Subject)
            .IsRequired()
            .HasMaxLength(250);
    }

    public DbSet<Comment> Comments { get; set; }
}
