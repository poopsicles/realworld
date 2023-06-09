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
        /// On success: A <c>UserResponse</c> containing the user's information and a JWT token <br />
        /// On failure: A 401 Unauthorized response
        /// </returns>
        [HttpGet, Authorize, Route("/api/User")]
        public async Task<IActionResult> GetUserInfo()
        {
            // [Authorize] handles this, but to make unit testing easier
            if (User == null)
            {
                return Unauthorized();
            }

            // get the username from the claims
            var username = User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
                .First().Value;
            var requestedUser = await _context.Users.FirstAsync(u => u.Username == username);

            // get the token from the headers
            // var token = await HttpContext.GetTokenAsync("access_token");
            var token = Request.Headers.Authorization.ToString()["Token ".Length..].Trim();

            var response = new UserResponse(requestedUser, token!);
            return Ok(response);
        }

        // GET: api/Users/5
        // [HttpGet("{id}")]
        // public async Task<ActionResult<UserModel>> GetUserModel(Guid id)
        // {
        //     if (_context.Users == null)
        //     {
        //         return NotFound();
        //     }
        //     var userModel = await _context.Users.FindAsync(id);

        //     if (userModel == null)
        //     {
        //         return NotFound();
        //     }

        //     return userModel;
        // }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutUserModel(Guid id, UserModel userModel)
        // {
        //     if (id != userModel.ID)
        //     {
        //         return BadRequest();
        //     }

        //     _context.Entry(userModel).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!UserModelExists(id))
        //         {
        //             return NotFound();
        //         }
        //         else
        //         {
        //             throw;
        //         }
        //     }

        //     return NoContent();
        // }

        /// <summary>
        /// POST: api/Users<br />
        /// Registers a user, with a unique username and email
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// On success: A <c>UserResponse</c> containing the user's information and a JWT token<br />
        /// On failure: An <c>ErrorResponse</c> containing a list of errors
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
        {
            if (_context.Users == null) // ensure database is set up correctly
            {
                return Problem("Entity set 'DatabaseContext.Users' is null.");
            }

            var errorlist = await request.Validate(_context); // validate request

            // if any errors were found, return them
            if (errorlist.Errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            // create new user, store, and generate token
            var newUser = new UserModel()
            {
                Username = request.User.Username,
                Password = request.User.Password,
                Email = request.User.Email
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

            var errorlist = request.ValidateWhitespace(); // validate request

            // if any whitespace errors were found, return them
            if (errorlist.Errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            var requestedUser = await _context.Users
                .Where(u => u.Email.ToLower() == request.User.Email.ToLower())
                .FirstOrDefaultAsync();

            errorlist = request.ValidateUser(requestedUser); // validate request

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


        // DELETE: api/Users/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteUserModel(Guid id)
        // {
        //     if (_context.Users == null)
        //     {
        //         return NotFound();
        //     }
        //     var userModel = await _context.Users.FindAsync(id);
        //     if (userModel == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.Users.Remove(userModel);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        private bool UserModelExists(Guid id)
        {
            return (_context.Users?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
