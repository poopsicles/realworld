using Microsoft.EntityFrameworkCore;
using Realworld.Models;

namespace Realworld.Contexts;

public class DatabaseContext : DbContext
{
    public DbSet<UserModel> Users { get; set; } = null!;
    public DbSet<ArticleModel> Articles { get; set; } = null!;
    public DbSet<CommentModel> Comments { get; set; } = null!;
    public DbSet<TagModel> Tags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>() // M:N users to usersFollowing
            .HasMany(u => u.usersFollowing)
            .WithMany()
            .UsingEntity(
                "UserFollowing",
                l => l.HasOne(typeof(UserModel)).WithMany().HasForeignKey("userFollowingID"),
                r => r.HasOne(typeof(UserModel)).WithMany().HasForeignKey("userID")
            );

        modelBuilder.Entity<UserModel>() // M:N users to favouriteArticles
            .HasMany(u => u.favouriteArticles)
            .WithMany()
            .UsingEntity(
                "UserFavouriteArticles",
                l => l.HasOne(typeof(ArticleModel)).WithMany().HasForeignKey("articleID"),
                r => r.HasOne(typeof(UserModel)).WithMany().HasForeignKey("userID")
            );

        modelBuilder.Entity<ArticleModel>() // 1:M article to comments
            .HasMany(a => a.comments)
            .WithOne(c => c.article)
            .HasForeignKey(c => c.articleID)
            .IsRequired();

        modelBuilder.Entity<ArticleModel>() // 1:M articles to author
            .HasOne(a => a.author)
            .WithMany()
            .HasForeignKey(a => a.authorID)
            .IsRequired();

        modelBuilder.Entity<ArticleModel>() // M:N articles to tags
            .HasMany(a => a.tags)
            .WithMany()
            .UsingEntity(
                "ArticleTags",
                l => l.HasOne(typeof(TagModel)).WithMany().HasForeignKey("tagID"),
                r => r.HasOne(typeof(ArticleModel)).WithMany().HasForeignKey("articleID")
            );

        modelBuilder.Entity<CommentModel>() // 1:M comments to author
            .HasOne(c => c.author)
            .WithMany()
            .HasForeignKey(c => c.authorID)
            .IsRequired();
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
}