using System.ComponentModel.DataAnnotations;

namespace Realworld.Models;

public class RegisterUserRequest
{
    public Components user { get; set; } = null!;
    public class Components
    {
        public string username { get; set; } = null!;
        public string email { get; set; } = null!;
        public string password { get; set; } = null!;
    }
}
