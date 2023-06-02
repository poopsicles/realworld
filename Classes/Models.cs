namespace Realworld.Models;

public class UserModel
{
    public Guid ID { get; set; }
    public string username { get; set; } = null!;
    public string password { get; set; } = null!;
    public string email { get; set; } = null!;
    public string bio { get; set; } = "";
    public Uri image { get; set; } = new Uri("https://static.productionready.io/images/smiley-cyrus.jpg");
    public List<UserModel> usersFollowing { get; set; } = new(); // Collection nav. of users followed, M:N
    public List<ArticleModel> favouriteArticles { get; set; } = new(); // Collection nav. of fave articles, M:N
}

public class ArticleModel
{
    public int ID { get; set; }
    public string title { get; set; } = null!;
    public string slug { get; set; } = null!;
    public string description { get; set; } = null!;
    public string body { get; set; } = null!;
    public Guid authorID { get; set; } // FK of author, 1:M
    public UserModel author { get; set; } = null!; // Reference nav. for author, 1:M
    public List<TagModel> tags { get; set; } = new(); // Collection nav. for tags, M:N
    public DateTime createdAt { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime updatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    public int favoritesCount { get; set; }
    public List<CommentModel> comments { get; set; } = new List<CommentModel>(); // Collection nav. for comments, 1:M
}

public class CommentModel
{
    public int ID { get; set; }
    public string body { get; set; } = null!;
    public Guid authorID { get; set; } // FK for comment author, 1:M
    public UserModel author { get; set; } = null!; // Reference nav. for comment author, 1:M
    public int articleID { get; set; } // FK for parent article, 1:M
    public ArticleModel article { get; set; } = null!; // Reference nav. for parent article, 1:M
    public DateTime createdAt { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime updatedAt { get; set; } = DateTime.Now.ToUniversalTime();
}

public class TagModel
{
    public int ID { get; set; }
    public string name { get; set; } = null!;
}