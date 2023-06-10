using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Realworld.Controllers;
using Realworld.Models;
using Realworld.Services;
using Xunit;

namespace realworld_unit_tests;
public class UpdateUserInfoTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<DatabaseContext> _contextOptions;
    private readonly TestTokenService _tokenService;

    public UpdateUserInfoTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(_connection)
            .Options;

        _tokenService = new TestTokenService();

        using var context = new DatabaseContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    DatabaseContext CreateContext() => new(_contextOptions);

    public void Dispose() => _connection.Dispose();

    [Fact(DisplayName = "PUT user while unauthorised returns 404")]
    public async Task UpdateUserInfoWhileUnauthorised_Returns401()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new UsersController(context, _tokenService);

        // Act
        var result = await controller.GetUserInfo();

        // Assert
        var objectResult = Assert.IsType<UnauthorizedResult>(result); // Ensures we got a 401
    }

    [Theory(DisplayName = "PUT user with whitespace returns 422")]
    [InlineData("", null, "newpass", null, null, new string[] { "username" })]
    [InlineData("", "", null, null, "i'm a fun person", new string[] { "username", "email" })]
    [InlineData("", "", "", "", "", new string[] { "username", "email", "password", "image", "bio" })]
    public async Task SupplyWhitespaceElements_Returns422(string? username, string? email, string? password, string? image, string? bio, string[] missing)
    {
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            Username = "testuser",
            Email = "test@me.com",
            Password = "password123",
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);

        // Authenticate 
        var login = await controller.LoginUser(new LoginUserRequest()
        {
            User = new LoginUserRequest.Components()
            {
                Email = "test@me.com",
                Password = "password123"
            }
        });

        var loginResult = Assert.IsType<OkObjectResult>(login);
        var token = Assert.IsType<UserResponse>(loginResult.Value).User.Token;

        // Set credentials in controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(_tokenService.GetTokenClaims(token))
                ),
            }
        };
        controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = $"Token {token}";

        var request = new UpdateUserRequest()
        {
            User = new UpdateUserRequest.Components()
            {
                Username = username,
                Email = email,
                Password = password,
                Image = image,
                Bio = bio
            }
        };

        // Act
        var result = await controller.UpdateUserInfo(request);

        // Assert
        var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result); // Got a 422
        var errorlist = Assert.IsType<ErrorResponse>(objectResult.Value); // Got an ErrorResponse

        // Ensures the correct errors are in the ErrorResponse
        Assert.Equal(missing.Count(), errorlist.Errors.Count);
        foreach (var elem in missing)
        {
            Assert.Contains(elem, errorlist.Errors.Keys);
            Assert.Equal("can't be empty", errorlist.Errors[elem]);
        }
    }

    [Theory(DisplayName = "PUT user with existing case-insensitive username/email returns 422")]
    [InlineData("totallydifferent", null, null, null, null, new string[] { "username" })]
    [InlineData(null, "me@me.com", null, null, null, new string[] { "email" })]
    [InlineData("totallydifferent", "me@me.com", null, null, null, new string[] { "username", "email" })]
    public async Task SupplyAlreadyExistingElements_Returns422(string? username, string? email, string? password, string? image, string? bio, string[] duplicate)
    {
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            Username = "testuser",
            Email = "test@me.com",
            Password = "password123",
        });
        context.Users.Add(new UserModel()
        {
            Username = "totallydifferent",
            Email = "test@diff.com",
            Password = "pass",
        });
        context.Users.Add(new UserModel()
        {
            Username = "MEEEE",
            Email = "me@me.com",
            Password = "12345678",
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);

        // Authenticate 
        var login = await controller.LoginUser(new LoginUserRequest()
        {
            User = new LoginUserRequest.Components()
            {
                Email = "test@me.com",
                Password = "password123"
            }
        });

        var loginResult = Assert.IsType<OkObjectResult>(login);
        var token = Assert.IsType<UserResponse>(loginResult.Value).User.Token;

        // Set credentials in controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(_tokenService.GetTokenClaims(token))
                ),
            }
        };
        controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = $"Token {token}";

        var request = new UpdateUserRequest()
        {
            User = new UpdateUserRequest.Components()
            {
                Username = username,
                Email = email,
                Password = password,
                Image = image,
                Bio = bio
            }
        };

        // Act
        var result = await controller.UpdateUserInfo(request);

        // Assert
        var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result); // Got a 422
        var errorlist = Assert.IsType<ErrorResponse>(objectResult.Value); // Got an ErrorResponse

        // Ensures the correct errors are in the ErrorResponse
        Assert.Equal(duplicate.Count(), errorlist.Errors.Count);
        foreach (var elem in duplicate)
        {
            Assert.Contains(elem, errorlist.Errors.Keys);
            Assert.Equal("already exists in database", errorlist.Errors[elem]);
        }
    }

    [Theory(DisplayName = "PUT user with valid elements returns 200")]
    [InlineData("testy", null, null, null, null)]
    [InlineData(null, "testuser@test.com", "nopass", null, null)]
    [InlineData("zesty", "testuser@test.com", "nopass", "https://example.com/image", "uwu")]
    public async Task SupplyValidElements_Returns200(string? username, string? email, string? password, string? image, string? bio)
    {
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            Username = "testuser",
            Email = "test@me.com",
            Password = "password123",
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);

        // Authenticate 
        var login = await controller.LoginUser(new LoginUserRequest()
        {
            User = new LoginUserRequest.Components()
            {
                Email = "test@me.com",
                Password = "password123"
            }
        });

        var loginResult = Assert.IsType<OkObjectResult>(login);
        var token = Assert.IsType<UserResponse>(loginResult.Value).User.Token;

        // Set credentials in controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(_tokenService.GetTokenClaims(token))
                ),
            }
        };
        controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = $"Token {token}";

        var request = new UpdateUserRequest()
        {
            User = new UpdateUserRequest.Components()
            {
                Username = username,
                Email = email,
                Password = password,
                Image = image,
                Bio = bio
            }
        };

        // Act
        var result = await controller.UpdateUserInfo(request);

        // Assert
        var objectResult = Assert.IsType<OkObjectResult>(result); // Got a 201
        var response = Assert.IsType<UserResponse>(objectResult.Value); // Got an UserResponse

        if (username != null) { Assert.Equal(username, response.User.Username); }
        if (email != null) { Assert.Equal(email, response.User.Email); }
        if (password != null) { Assert.Equal(password, context.Users.First().Password); }
        if (image != null) { Assert.Equal(new Uri(image), response.User.Image); }
        if (bio != null) { Assert.Equal(bio, response.User.Bio); }

        switch (username == null, email == null) {
            case (true, true): // none changed
                Assert.True(_tokenService.ValidateToken(response.User.Token, response.User.Username, response.User.Email));
                break;
            
            case (true, false): // email changed
                Assert.True(_tokenService.ValidateToken(response.User.Token, response.User.Username, email!));
                break;
            
            case (false, true): // username changed
                Assert.True(_tokenService.ValidateToken(response.User.Token, username!, response.User.Email));
                break;
            
            case (false, false): // both changed
                Assert.True(_tokenService.ValidateToken(response.User.Token, username!, email!));
                break;
        }
    }

    [Theory(DisplayName = "PUT user with case-insensitive same elements returns 200")]
    [InlineData("testuser", "test@me.com", "password123", "https://example.com/image", "new bio")]
    [InlineData("TESTuser", "test@ME.COM", "password123", "https://example.com/image", "new bio")]
    public async Task SupplyOwnElements_Returns200(string username, string email, string password, string image, string bio)
    {
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            Username = "testuser",
            Email = "test@me.com",
            Password = "password123",
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);

        // Authenticate 
        var login = await controller.LoginUser(new LoginUserRequest()
        {
            User = new LoginUserRequest.Components()
            {
                Email = "test@me.com",
                Password = "password123"
            }
        });

        var loginResult = Assert.IsType<OkObjectResult>(login);
        var token = Assert.IsType<UserResponse>(loginResult.Value).User.Token;

        // Set credentials in controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(_tokenService.GetTokenClaims(token))
                ),
            }
        };
        controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = $"Token {token}";

        var request = new UpdateUserRequest()
        {
            User = new UpdateUserRequest.Components()
            {
                Username = username,
                Email = email,
                Password = password,
                Image = image,
                Bio = bio
            }
        };

        // Act
        var result = await controller.UpdateUserInfo(request);

        // Assert
        var objectResult = Assert.IsType<OkObjectResult>(result); // Got a 201
        var response = Assert.IsType<UserResponse>(objectResult.Value); // Got an UserResponse

        Assert.Equal(username, response.User.Username);
        Assert.Equal(email, response.User.Email);
        Assert.Equal(password, context.Users.First().Password);
        Assert.Equal(new Uri(image), response.User.Image);
        Assert.Equal(bio, response.User.Bio);
    }
}
