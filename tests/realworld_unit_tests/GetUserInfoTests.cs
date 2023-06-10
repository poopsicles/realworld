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
public class GetUserInfoTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<DatabaseContext> _contextOptions;
    private readonly TestTokenService _tokenService;

    public GetUserInfoTests()
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

    [Fact(DisplayName = "GET user info while unauthorised returns 401")]
    public async Task GetUserInfoWhileUnauthorised_Returns401()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new UsersController(context, _tokenService);

        // Act
        var result = await controller.GetUserInfo();

        // Assert
        var objectResult = Assert.IsType<UnauthorizedResult>(result); // Ensures we got a 401
    }

    [Theory(DisplayName = "GET user info while authorised returns 200")]
    [InlineData("test@user.com", "password")]
    [InlineData("TEST@user.com", "password")]
    public async Task GetUserInfoWhileAuthorised_Returns200(string email, string password)
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            Username = "Testuser",
            Email = "test@user.com",
            Password = "password",
            Bio = "uwu",
            Image = new Uri("https://example.com/image")
        });
        context.SaveChanges();


        var controller = new UsersController(context, _tokenService);

        // Authenticate 
        var login = await controller.LoginUser(new LoginUserRequest()
        {
            User = new LoginUserRequest.Components()
            {
                Email = email,
                Password = password
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

        // Act
        var result = await controller.GetUserInfo();

        // Assert 
        var objectResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponse>(objectResult.Value);

        Assert.Equal("Testuser", response.User.Username);
        Assert.Equal("test@user.com", response.User.Email);
        Assert.Equal(new Uri("https://example.com/image"), response.User.Image);
        Assert.Equal("uwu", response.User.Bio);
        Assert.Equal(token, response.User.Token);
    }
}