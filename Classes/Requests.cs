using System.ComponentModel.DataAnnotations;

namespace Realworld.Requests;

public class RegisterUserRequest
{
    public RegisterUserRequestComponents user { get; set; } = null!;
    public class RegisterUserRequestComponents
    {
        public string username { get; set; } = null!;
        public string email { get; set; } = null!;
        public string password { get; set; } = null!;
    }
}
