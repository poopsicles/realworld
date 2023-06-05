using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Realworld.Services;

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

    public async Task<ErrorResponse> Validate(DatabaseContext context)
    {
        var errorlist = new ErrorResponse();

        if (String.IsNullOrWhiteSpace(user.username)) // validate username
        {
            errorlist.errors.Add("username", "can't be empty");
        }

        if (String.IsNullOrWhiteSpace(user.email)) // validate email
        {
            errorlist.errors.Add("email", "can't be empty");
        }

        if (String.IsNullOrWhiteSpace(user.password)) // validate password
        {
            errorlist.errors.Add("password", "can't be empty");
        }

        // ensure username is not already taken
        if (await context.Users.FirstOrDefaultAsync(u => u.username == user.username) != null)
        {
            errorlist.errors.Add("username", "already exists in database");
        }

        // ensure email is not already taken
        if (await context.Users.FirstOrDefaultAsync(u => u.email == user.email) != null)
        {
            errorlist.errors.Add("email", "already exists in database");
        }

        return errorlist;
    }
}

public class LoginUserRequest {
    public Components user { get; set; } = null!;

    public class Components {
        public string email { get; set; } = null!;
        public string password { get; set; } = null!;
    }

    public ErrorResponse ValidateWhitespace()
    {
        var errorlist = new ErrorResponse();

        if (String.IsNullOrWhiteSpace(user.email)) // validate email
        {
            errorlist.errors.Add("email", "can't be empty");
        }

        if (String.IsNullOrWhiteSpace(user.password)) // validate password
        {
            errorlist.errors.Add("password", "can't be empty");
        }

        return errorlist;
    }

    public ErrorResponse ValidateUser(UserModel? requestedUser)
    {
        var errorlist = new ErrorResponse();

        if (requestedUser == null)
        {
            errorlist.errors.Add("email", "doesn't exist in database");
            return errorlist;
        } 
        
        if (requestedUser.password != user.password) {
            errorlist.errors.Add("password", "is incorrect");
        }

        return errorlist;
    }
}