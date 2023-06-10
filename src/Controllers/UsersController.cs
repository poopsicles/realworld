using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Realworld.Services;
using Realworld.Models;
using Microsoft.AspNetCore.Authentication;

namespace Realworld.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ITokenService _tokenService;

        public UsersController(DatabaseContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// GET: api/User<br />
        /// Gets an authorised user's information from a JWT token
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// If successful, an <c>UserResponse</c> containing the user's information and a JWT token <br />
        /// Else, a 401 Unauthorized response
        /// </returns>
        [HttpGet, Authorize, Route("/api/User")]
        public async Task<IActionResult> GetUserInfo()
        {
            // [Authorize] handles this, but to make unit testing easier
            if (User == null)
            {
                return Unauthorized();
            }

            var requestedUser = await GetRequestedUserFromClaims();

            if (requestedUser == null)
            {
                return Unauthorized();
            }

            // var token = await HttpContext.GetTokenAsync("access_token");
            // reuse old token
            var token = GetTokenFromHeaders();

            var response = new UserResponse(requestedUser, token!);
            return Ok(response);
        }

        /// <summary>
        /// PUT: api/User<br />
        /// Updates an authorised user's information
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// If unauthorised, a 401 Unauthrorized response
        /// If successful, an <c>UserResponse</c> containing the user's information and a JWT token <br />
        /// Else, an <c>ErrorResponse</c> containing a list of errors
        /// </returns>
        [HttpPut, Authorize, Route("/api/User")]
        public async Task<IActionResult> UpdateUserInfo(UpdateUserRequest request)
        {
            // [Authorize] handles this, but to make unit testing easier
            if (User == null)
            {
                return Unauthorized();
            }

            var requestedUser = await GetRequestedUserFromClaims();

            if (requestedUser == null)
            {
                return Unauthorized();
            }

            var errorlist = request.ValidateWhitespace();

            // if any errors were found, return them
            if (errorlist.Errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            errorlist = await request.ValidateUniqueness(requestedUser, _context); 

            // if any errors were found, return them
            if (errorlist.Errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            // update user
            if (request.User.Username != null) { requestedUser.Username = request.User.Username.Trim(); }
            if (request.User.Email != null) { requestedUser.Email = request.User.Email.Trim(); }
            if (request.User.Password != null) { requestedUser.Password = request.User.Password; }
            if (request.User.Image != null) { requestedUser.Image = new Uri(request.User.Image); }
            if (request.User.Bio != null) { requestedUser.Bio = request.User.Bio.Trim(); }

            await _context.SaveChangesAsync();

            // regenerate token with new claims
            var token = _tokenService.CreateToken(requestedUser);
            
            return Ok(new UserResponse(requestedUser, token));
        }

        /// <summary>
        /// POST: api/Users<br />
        /// Registers a user, with a unique username and email
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// If successful, an <c>UserResponse</c> containing the user's information and a JWT token
        /// Else, an <c>ErrorResponse</c> containing a list of errors
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
        {
            if (_context.Users == null) // ensure database is set up correctly
            {
                return Problem("Entity set 'DatabaseContext.Users' is null.");
            }

            var errorlist = await request.Validate(_context);

            // return any found errors
            if (errorlist.Errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            // create new user, store, and generate token
            var newUser = new UserModel()
            {
                Username = request.User.Username.Trim(),
                Password = request.User.Password,
                Email = request.User.Email.Trim()
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(newUser);

            // 201 with new user
            return CreatedAtAction(nameof(RegisterUser), new UserResponse(newUser, token));
        }

        /// <summary>
        /// POST: api/Users/login<br />
        /// Logs in a user, with a supplied username and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// On success: A <c>UserResponse</c> containing the user's information and a JWT token<br />
        /// On failure: An <c>ErrorResponse</c> containing a list of errors
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginUserRequest request)
        {
            if (_context.Users == null) // ensure database is set up correctly
            {
                return Problem("Entity set 'DatabaseContext.Users' is null.");
            }

            var errorlist = request.ValidateWhitespace();

            // return any found whitespace errors
            if (errorlist.Errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            var requestedUser = await _context.Users
                .Where(u => u.Email.ToLower() == request.User.Email.ToLower())
                .FirstOrDefaultAsync();

            errorlist = request.ValidateSemantics(requestedUser);

            // return any found semantic errors
            if (errorlist.Errors.Count != 0)
            {
                // if the user doesn't exist
                if (errorlist.Errors.ContainsValue("doesn't exist in database"))
                {
                    return NotFound(errorlist); // 404 with errors
                }

                return UnprocessableEntity(errorlist); // 422 with errors
            }

            var token = _tokenService.CreateToken(requestedUser!);

            // 200 with user
            return Ok(new UserResponse(requestedUser!, token));
        }

        private bool UserModelExists(Guid id)
        {
            return (_context.Users?.Any(e => e.ID == id)).GetValueOrDefault();
        }

        private string GetTokenFromHeaders()
        {
            return Request.Headers.Authorization.ToString()["Token ".Length..].Trim();
        }

        private async Task<UserModel?> GetRequestedUserFromClaims()
        {
            var claimedUsername = User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
                .First().Value;

            var claimedID = Guid.Parse(User.Claims
                .Where(c => c.Type == "Id")
                .First().Value);

            var claimedEmail = User.Claims
                .Where(c => c.Type == ClaimTypes.Email || c.Type == "email")
                .First().Value;

            var user = await _context.Users.FirstAsync(u => u.ID == claimedID);

            // all the claims must match up
            if (user == null ||
                user.Username.ToLower() != claimedUsername.ToLower() ||
                user.Email.ToLower() != claimedEmail.ToLower())
            {
                return null;
            }

            return user;
        }
    }
}
