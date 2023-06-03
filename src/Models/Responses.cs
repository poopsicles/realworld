using Realworld.Models;

namespace Realworld.Models;

public class UserResponse {
    public Components user { get; set; } = null!;

    public class Components {
        public string email { get; set; } = null!;
        public string token { get; set; } = null!;
        public string username { get; set; } = null!;
        public string bio { get; set; } = null!;
        public Uri image { get; set; } = null!;
    }

    public UserResponse(UserModel usermodel, string token) {
        user = new Components() {
            email = usermodel.email,
            username = usermodel.username,
            bio = usermodel.bio,
            image = usermodel.image,
            token = token
        };
    }
}

public class ProfileResponse {
    public Components profile { get; set; } = null!;

    public class Components {
        public string username { get; set; } = null!;
        public string bio { get; set; } = null!;
        public Uri image { get; set; } = null!;
        public bool following { get; set; }
    }
}

public class ArticleResponse {
    public Components article { get; set; } = null!;

    public class Components{
        public string slug { get; set; } = null!;
        public string title { get; set; } = null!;
        public string description { get; set; } = null!;
        public string body { get; set; } = null!;
        public List<string> tagList { get; set; } = null!;
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool favorited { get; set; }
        public int favoritesCount { get; set; }
        public ProfileResponse.Components author { get; set; } = null!;
    }
}

public class ArticleListResponse {
    public List<ArticleResponse> articles { get; set; } = null!;
    public int articlesCount { get; set; }
}

public class CommentResponse {
    public Components comment { get; set; } = null!;

    public class Components {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string body { get; set; } = null!;
        public ProfileResponse.Components author { get; set; } = null!;
    }
}

public class ErrorResponse {
    public Dictionary<string, string> errors { get; set; } = new();
}