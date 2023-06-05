using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Realworld.Services;
using Realworld.Models;

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

        // GET: api/Users
        // [HttpGet, Authorize]
        // public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers()
        // {
        //     if (_context.Users == null)
        //     {
        //         return NotFound();
        //     }
        //     return await _context.Users.ToListAsync();
        // }

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

        // POST: api/Users
        // Registers a user, with a unique username and email
        // Success: Returns a UserResponse containing the user's information and a JWT token
        // Failure: Returns an ErrorResponse containing a list of errors
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
        {
            if (_context.Users == null) // ensure database is set up correctly
            {
                return Problem("Entity set 'DatabaseContext.Users' is null.");
            }

            var errorlist = await request.Validate(_context); // validate request

            // if any errors were found, return them
            if (errorlist.errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            // create new user, store, and generate token
            var newUser = new UserModel()
            {
                username = request.user.username,
                password = request.user.password,
                email = request.user.email
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(newUser);

            // 201 with new user
            return CreatedAtAction(nameof(RegisterUser), new UserResponse(newUser, token));
        }

        // POST: api/Users/login
        // Logs in a user, with a supplied username and password
        // Success: Returns a UserResponse containing the user's information and a JWT token
        // Failure: Returns an ErrorResponse containing a list of errors
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginUserRequest request) {
            if (_context.Users == null) // ensure database is set up correctly
            {
                return Problem("Entity set 'DatabaseContext.Users' is null.");
            }

            var errorlist = request.ValidateWhitespace(); // validate request

            // if any whitespace errors were found, return them
            if (errorlist.errors.Count != 0)
            {
                return UnprocessableEntity(errorlist); // 422 with errors
            }

            var requestedUser = await _context.Users.Where(u => u.email == request.user.email).FirstOrDefaultAsync();

            errorlist = request.ValidateUser(requestedUser); // validate request
            
            if (errorlist.errors.Count != 0)
            {
                // if the user doesn't exist
                if (errorlist.errors.ContainsValue("doesn't exist in database")) {
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

        // private bool UserModelExists(Guid id)
        // {
        //     return (_context.Users?.Any(e => e.ID == id)).GetValueOrDefault();
        // }
    }
}
