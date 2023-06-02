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
        private readonly TokenService _tokenService;

        public UsersController(DatabaseContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // GET: api/Users
        [HttpGet, Authorize]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            return await _context.Users.ToListAsync();
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserResponse>> RegisterUser(RegisterUserRequest request)
        {
            if (_context.Users == null) // ensure database is set up correctly
            {
                return Problem("Entity set 'DatabaseContext.Users' is null.");
            }

            var errorlist = new ErrorResponse();

            if (String.IsNullOrWhiteSpace(request.user.username)) // validate username
            {
                errorlist.errors.Add("username", "can't be empty");
            }

            if (String.IsNullOrWhiteSpace(request.user.email)) // validate email
            {
                errorlist.errors.Add("email", "can't be empty");
            }

            if (String.IsNullOrWhiteSpace(request.user.password)) // validate password
            {
                errorlist.errors.Add("password", "can't be empty");
            }

            // ensure username is not already taken
            if (await _context.Users.FirstOrDefaultAsync(u => u.username == request.user.username) != null)
            {
                errorlist.errors.Add("username", "already exists in database");
            }

            // ensure email is not already taken
            if (await _context.Users.FirstOrDefaultAsync(u => u.email == request.user.email) != null)
            {
                errorlist.errors.Add("email", "already exists in database");
            }

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
