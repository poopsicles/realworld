using Realworld.Models;

namespace Realworld.Responses;

public class UserResponse {
    public string email { get; set; }
    public string token { get; set; }
    public string username { get; set; }
    public string bio { get; set; }
    public Uri image { get; set; }

    public UserResponse(UserModel user, string token) {
        email = user.email;
        username = user.username;
        bio = user.bio;
        image = user.image;
        this.token = token;
    }
}

public class ProfileResponse {
    public string username { get; set; } = null!;
    public string bio { get; set; } = null!;
    public Uri image { get; set; } = null!;
    public bool following { get; set; }
}

public class ArticleResponse {
    public string slug { get; set; } = null!;
    public string title { get; set; } = null!;
    public string description { get; set; } = null!;
    public string body { get; set; } = null!;
    public List<string> tagList { get; set; } = null!;
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public bool favorited { get; set; }
    public int favoritesCount { get; set; }
    public ProfileResponse author { get; set; } = null!;
}

public class ArticleListResponse {
    public List<ArticleResponse> articles { get; set; } = null!;
    public int articlesCount { get; set; }
}

public class CommentResponse {
    public int id { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public string body { get; set; } = null!;
    public ProfileResponse author { get; set; } = null!;
}

public class ErrorResponse {
    public Dictionary<string, string> errors { get; set; } = new();
}