using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Realworld.Controllers;
using Realworld.Models;
using Realworld.Services;
using Xunit;
using Xunit.Abstractions;

namespace realworld_unit_tests;

public class LoginUserTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<DatabaseContext> _contextOptions;
    private readonly TestTokenService _tokenService;

    public LoginUserTests()
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

    DatabaseContext CreateContext() => new DatabaseContext(_contextOptions);

    public void Dispose() => _connection.Dispose();

    [Theory]
    [InlineData("", "testpassword", new string[] { "email" })]
    [InlineData("testuser@office.com", "", new string[] { "password" })]
    [InlineData("", "", new string[] { "email", "password" })]
    public async Task SupplyWhitespaceElements_Returns422(string email, string password, string[] missing)
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            email = "testuser@office.com",
            username = "testuser",
            password = "testpassword"
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);
        var request = new LoginUserRequest()
        {
            user = new LoginUserRequest.Components()
            {
                email = email,
                password = password
            }
        };

        // Act
        var result = await controller.LoginUser(request);

        // Assert
        var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result); // Ensures we got a 422
        var errorlist = Assert.IsType<ErrorResponse>(objectResult.Value); // Ensures we got an ErrorResponse

        // Ensures the correct errors are in the ErrorResponse
        Assert.Equal(missing.Count(), errorlist.errors.Count);
        foreach (var elem in missing)
        {
            Assert.Contains(elem, errorlist.errors.Keys);
            Assert.Equal("can't be empty", errorlist.errors[elem]);
        }
    }

    [Fact]
    public async Task SupplyInvalidEmail_Returns404()
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            email = "testuser@office.com",
            username = "testuser",
            password = "testpassword"
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);
        var request = new LoginUserRequest()
        {
            user = new LoginUserRequest.Components()
            {
                email = "me@me.com",
                password = "password"
            }
        };

        // Act
        var result = await controller.LoginUser(request);

        // Assert
        var objectResult = Assert.IsType<NotFoundObjectResult>(result); // Ensures we got a 404
        var errorlist = Assert.IsType<ErrorResponse>(objectResult.Value); // Ensures we got an ErrorResponse

        // Ensures the correct error is in the ErrorResponse
        Assert.Equal(new KeyValuePair<string, string>("email", "doesn't exist in database"), errorlist.errors.First()); 
    }

    [Fact]
    public async Task SupplyInvalidPassword_Returns422()
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            username = "testuser",
            email = "me@me.com",
            password = "password123"
        });

        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);
        var request = new LoginUserRequest()
        {
            user = new LoginUserRequest.Components()
            {
                email = "me@me.com",
                password = "password"
            }
        };

        // Act
        var result = await controller.LoginUser(request);

        // Assert
        var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result); // Ensures we got a 422
        var errorlist = Assert.IsType<ErrorResponse>(objectResult.Value); // Ensures we got an ErrorResponse

        // Ensures the correct error is in the ErrorResponse
        Assert.Equal(new KeyValuePair<string, string>("password", "is incorrect"), errorlist.errors.First());
    }

    [Theory]
    [InlineData("testuser", "me@me.com", "password123")]
    [InlineData("admin", "admin@me.com", "root")]
    public async Task SupplyValidElements_Returns200(string username, string email, string password)
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new UserModel()
        {
            username = "testuser",
            email = "me@me.com",
            password = "password123"
        });
        context.Users.Add(new UserModel()
        {
            username = "admin",
            email = "admin@me.com",
            password = "root"
        });

        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);
        var request = new LoginUserRequest()
        {
            user = new LoginUserRequest.Components()
            {
                email = email,
                password = password
            }
        };

        // Act
        var result = await controller.LoginUser(request);

        // Assert
        var objectResult = Assert.IsType<OkObjectResult>(result); // Ensures we got a 422
        var response = Assert.IsType<UserResponse>(objectResult.Value); // Ensures we got an ErrorResponse

        Assert.Equal(2, context.Users.Count());
        Assert.Equal(email, response.user.email);
        Assert.True(_tokenService.ValidateToken(response.user.token, username, email));
    }
}