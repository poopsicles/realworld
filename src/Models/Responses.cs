using Realworld.Models;

namespace Realworld.Models;

public class UserResponse {
    public Components User { get; set; } = null!;

    public class Components {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public Uri Image { get; set; } = null!;
    }

    public UserResponse(UserModel usermodel, string token) {
        User = new Components() {
            Email = usermodel.Email,
            Username = usermodel.Username,
            Bio = usermodel.Bio,
            Image = usermodel.Image,
            Token = token
        };
    }
}

public class ProfileResponse {
    public Components Profile { get; set; } = null!;

    public class Components {
        public string Username { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public Uri Image { get; set; } = null!;
        public bool Following { get; set; }
    }
}

public class ArticleResponse {
    public Components Article { get; set; } = null!;

    public class Components{
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Body { get; set; } = null!;
        public List<string> TagList { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Favorited { get; set; }
        public int FavoritesCount { get; set; }
        public ProfileResponse.Components Author { get; set; } = null!;
    }
}

public class ArticleListResponse {
    public List<ArticleResponse> Articles { get; set; } = null!;
    public int ArticlesCount { get; set; }
}

public class CommentResponse {
    public Components Comment { get; set; } = null!;

    public class Components {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Body { get; set; } = null!;
        public ProfileResponse.Components Author { get; set; } = null!;
    }
}

public class ErrorResponse {
    public Dictionary<string, string> Errors { get; set; } = new();
}