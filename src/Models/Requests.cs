using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Realworld.Services;

namespace Realworld.Models;

/// <summary>
/// A request body for registering a new user
/// </summary>
public class RegisterUserRequest
{
    public Components User { get; set; } = null!;
    public class Components
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// Validates the request body for whitespace and uniqueness errors
    /// </summary>
    /// <param name="context"></param>
    /// <returns>An <c>ErrorResponse</c> containing any errors</returns>
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
        if (await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == User.Username.ToLower().Trim()) != null)
        {
            errorlist.Errors.Add("username", "already exists in database");
        }

        // ensure email is not already taken
        if (await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == User.Email.ToLower().Trim()) != null)
        {
            errorlist.Errors.Add("email", "already exists in database");
        }

        return errorlist;
    }
}

/// <summary>
/// A request body for logging in a user
/// </summary>
public class LoginUserRequest
{
    public Components User { get; set; } = null!;
    public class Components
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// Validates the request body for whitespace errors
    /// </summary>
    /// <returns>An <c>ErrorResponse</c> containing any errors</returns>
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

    /// <summary>
    /// Validates the request body for uniqueness and semantic errors
    /// </summary>
    /// <returns>An <c>ErrorResponse</c> containing any errors</returns>
    public ErrorResponse ValidateSemantics(UserModel? requestedUser)
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

/// <summary>
/// A request body for updating a user's info
/// </summary>
public class UpdateUserRequest
{
    public Components User { get; set; } = null!;
    public class Components
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Image { get; set; }
        public string? Bio { get; set; }
    }

    /// <summary>
    /// Validates the request body for whitespace errors
    /// </summary>
    /// <returns>An <c>ErrorResponse</c> containing any errors</returns>
    public ErrorResponse ValidateWhitespace()
    {
        var errorlist = new ErrorResponse();

        if (User.Username?.Trim() == String.Empty) // validate username
        {
            errorlist.Errors.Add("username", "can't be empty");
        }

        if (User.Email?.Trim() == String.Empty) // validate email
        {
            errorlist.Errors.Add("email", "can't be empty");
        }

        if (User.Password?.Trim() == String.Empty) // validate password
        {
            errorlist.Errors.Add("password", "can't be empty");
        }

        if (User.Image?.Trim() == String.Empty) // validate image
        {
            errorlist.Errors.Add("image", "can't be empty");
        }

        if (User.Bio?.Trim() == String.Empty) // validate password
        {
            errorlist.Errors.Add("bio", "can't be empty");
        }

        return errorlist;
    }

    public async Task<ErrorResponse> ValidateUniqueness(UserModel requestedUser, DatabaseContext context) {
        var errorlist = new ErrorResponse();

        // if username is being changed, ensure it is not already taken
        if (User.Username != null) {
            if (requestedUser.Username.ToLower() != User.Username.ToLower())
            {
                if (await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == User.Username.ToLower()) != null)
                {
                    errorlist.Errors.Add("username", "already exists in database");
                }
            }
        }

        // if email is being changed, ensure it is not already taken
        if (User.Email != null) {
            if (requestedUser.Email.ToLower() != User.Email.ToLower())
            {
                if (await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == User.Email.ToLower()) != null)
                {
                    errorlist.Errors.Add("email", "already exists in database");
                }
            }
        }

        return errorlist;
    }

}