using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Realworld.Services;

namespace Realworld.Models;

public class RegisterUserRequest
{
    public Components User { get; set; } = null!;
    public class Components
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public async Task<ErrorResponse> Validate(DatabaseContext context)
    {
        var errorlist = new ErrorResponse();

        if (String.IsNullOrWhiteSpace(User.Username)) // validate username
        {
            errorlist.Errors.Add("username", "can't be empty");
        }

        if (String.IsNullOrWhiteSpace(User.Email)) // validate email
        {
            errorlist.Errors.Add("email", "can't be empty");
        }

        if (String.IsNullOrWhiteSpace(User.Password)) // validate password
        {
            errorlist.Errors.Add("password", "can't be empty");
        }

        // ensure username is not already taken
        if (await context.Users.FirstOrDefaultAsync(u => u.Username == User.Username) != null)
        {
            errorlist.Errors.Add("username", "already exists in database");
        }

        // ensure email is not already taken
        if (await context.Users.FirstOrDefaultAsync(u => u.Email == User.Email) != null)
        {
            errorlist.Errors.Add("email", "already exists in database");
        }

        return errorlist;
    }
}

public class LoginUserRequest
{
    public Components User { get; set; } = null!;
    public class Components
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public ErrorResponse ValidateWhitespace()
    {
        var errorlist = new ErrorResponse();

        if (String.IsNullOrWhiteSpace(User.Email)) // validate email
        {
            errorlist.Errors.Add("email", "can't be empty");
        }

        if (String.IsNullOrWhiteSpace(User.Password)) // validate password
        {
            errorlist.Errors.Add("password", "can't be empty");
        }

        return errorlist;
    }

    public ErrorResponse ValidateUser(UserModel? requestedUser)
    {
        var errorlist = new ErrorResponse();

        if (requestedUser == null)
        {
            errorlist.Errors.Add("email", "doesn't exist in database");
            return errorlist;
        }

        if (requestedUser.Password != User.Password)
        {
            errorlist.Errors.Add("password", "is incorrect");
        }

        return errorlist;
    }
}