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

public class RegisterUserTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<DatabaseContext> _contextOptions;
    private readonly TestTokenService _tokenService;
    
    public RegisterUserTests() {
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
    [InlineData("", "test@example.com", "testpassword", new string[] {"username"})]
    [InlineData("testuser", "", "testpassword", new string[] {"email"})]
    [InlineData("testuser", "test@example.com", "", new string[] {"password"})]
    [InlineData("", "", "", new string[] {"username", "email", "password"})]
    public async Task SupplyWhitespaceElements_Returns422(string username, string email, string password, string[] missing)
    {
        // Arrange
        using var context = CreateContext();
        var controller = new UsersController(context, _tokenService);
        var request = new RegisterUserRequest() {
            user = new RegisterUserRequest.Components() {
                username = username,
                email = email,
                password = password
            }
        };

        // Act
        var result = await controller.RegisterUser(request);

        // Assert
        var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result); // Ensures we got a 422
        var errorlist = Assert.IsType<ErrorResponse>(objectResult.Value); // Ensures we got an ErrorResponse

        // Ensures the correct errors are in the ErrorResponse
        Assert.Equal(missing.Count(), errorlist.errors.Count); 
        foreach (var elem in missing) {
            Assert.Contains(elem, errorlist.errors.Keys); 
            Assert.Equal("can't be empty", errorlist.errors[elem]);
        }
    }

    [Theory]
    [InlineData("testuser", "other@example.com", new string[] {"username"})]
    [InlineData("other", "test@example.com", new string[] {"email"})]
    [InlineData("testuser", "test@example.com", new string[] {"username", "email"})]
    public async Task SupplyDuplicateElements_Returns422(string username, string email, string[] duplicate) {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new UserModel() {
            username = "testuser",
            email = "test@example.com",
            password = "testpassword"
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);
        var request = new RegisterUserRequest() {
            user = new RegisterUserRequest.Components() {
                username = username,
                email = email,
                password = "password"
            }
        };

        // Act
        var result = await controller.RegisterUser(request);

        // Assert
        var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result); // Ensures we got a 422
        var errorlist = Assert.IsType<ErrorResponse>(objectResult.Value); // Ensures we got an ErrorResponse

        // Ensures the correct errors are in the ErrorResponse
        Assert.Equal(duplicate.Count(), errorlist.errors.Count); 
        foreach (var elem in duplicate) {
            Assert.Contains(elem, errorlist.errors.Keys); 
            Assert.Equal("already exists in database", errorlist.errors[elem]);
        }
    }

    [Theory]
    [InlineData("testuser", "test@example.com", "password")]
    [InlineData("other", "other@example.com", "123")]
    public async Task SupplyValidElements_Returns201(string username, string email, string password) {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new UserModel() {
            username = "admin",
            email = "admin@example.com",
            password = "root"
        });
        context.SaveChanges();

        var controller = new UsersController(context, _tokenService);
        var request = new RegisterUserRequest() {
            user = new RegisterUserRequest.Components() {
                username = username,
                email = email,
                password = password
            }
        };

        // Act
        var result = await controller.RegisterUser(request);

        // Assert
        var objectResult = Assert.IsType<CreatedAtActionResult>(result); // Ensures we got a 201
        var response = Assert.IsType<UserResponse>(objectResult.Value); // Ensures we got a UserResponse

        Assert.Equal(2, context.Users.Count());
        Assert.Equal(username, response.user.username);
        Assert.Equal(email, response.user.email);
        Assert.True(_tokenService.ValidateToken(response.user.token, username, email));
    }
}