namespace Realworld.Models;

public class UserModel
{
    public Guid ID { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Bio { get; set; } = "";
    public Uri Image { get; set; } = new Uri("https://static.productionready.io/images/smiley-cyrus.jpg");
    public List<UserModel> UsersFollowing { get; set; } = new(); // Collection nav. of users followed, M:N
    public List<ArticleModel> FavouriteArticles { get; set; } = new(); // Collection nav. of fave articles, M:N
}

public class ArticleModel
{
    public int ID { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Guid AuthorID { get; set; } // FK of author, 1:M
    public UserModel Author { get; set; } = null!; // Reference nav. for author, 1:M
    public List<TagModel> Tags { get; set; } = new(); // Collection nav. for tags, M:N
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int FavoritesCount { get; set; }
    public List<CommentModel> Comments { get; set; } = new List<CommentModel>(); // Collection nav. for comments, 1:M
}

public class CommentModel
{
    public int ID { get; set; }
    public string Body { get; set; } = null!;
    public Guid AuthorID { get; set; } // FK for comment author, 1:M
    public UserModel Author { get; set; } = null!; // Reference nav. for comment author, 1:M
    public int ArticleID { get; set; } // FK for parent article, 1:M
    public ArticleModel Article { get; set; } = null!; // Reference nav. for parent article, 1:M
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TagModel
{
    public int ID { get; set; }
    public string Name { get; set; } = null!;
}