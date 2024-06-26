using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManagment.Server.Data;
using TaskManagment.Server.DTOs;
using TaskManagment.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Json;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TaskManagment.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TaskManagmentServerContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(TaskManagmentServerContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            try
            {
                _logger.LogInformation("Retrieving all users...");
                var users = await _context.Users.ToListAsync();
                foreach (User user in users)
                {
                    if (_context.Tasks.Any(tasks => tasks.UserId == user.Id))
                    {
                        var foundTasks = _context.Tasks.Where(tasks => tasks.UserId == user.Id).ToList();
                        user.Tasks = foundTasks;
                    }
                }
                _logger.LogInformation("Successfully retrieved all users.");

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all users.");
                return StatusCode(500, "An error occurred while retrieving all users.");
            }
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving user with ID {UserId}...", id);
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved user with ID {UserId}.", id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user with ID {UserId}.", id);
                return StatusCode(500, "An error occurred while retrieving user.");
            }
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, LoginUserDto loginUserDto)
        {
            try
            {
                _logger.LogInformation("Updating user with ID {UserId}...", id);

                var user = await _context.Users.FindAsync(id);

                if (id != user.Id)
                {
                    _logger.LogWarning("Invalid request: Provided ID ({ProvidedId}) does not match user ID.", id);
                    return BadRequest("Invalid ID provided.");
                }

                if (VerifyPassword(loginUserDto.Password, user.PasswordHash))
                {
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User with ID {UserId} updated successfully.", id);
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning("Invalid password provided for user with ID {UserId}.", id);
                    return BadRequest("Invalid password provided.");
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user with ID {UserId}.", id);
                return StatusCode(500, "An error occurred while updating user.");
            }
        }


        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                _logger.LogInformation("Deleting user with ID {UserId}...", id);

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with ID {UserId}.", id);
                return StatusCode(500, "An error occurred while deleting user.");
            }
        }

        // POST: api/Users/register
        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(RegisterUserDto registerUserDto)
        {
            try
            {
                _logger.LogInformation("Registering a new user...");
                CreatePasswordHash(registerUserDto.Password, out string hashPasword);

                var user = await _context.Users.AddAsync(new User()
                {
                    Username = registerUserDto.Username,
                    Email = registerUserDto.Email,
                    PasswordHash = hashPasword
                });

                _logger.LogInformation($"User registered successfully.{user.Entity.Username}, {registerUserDto.Password}");
                return CreatedAtAction("GetUser", new { id = user.Entity.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the user.");
                return StatusCode(500, $"An error occurred while registering user.");
            }
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<ActionResult<User>> LoginUser(LoginUserDto loginUserDto)
        {
            try
            {
                _logger.LogInformation("Logging in user {Username}...", loginUserDto.Username);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginUserDto.Username);

                if (user == null || !VerifyPassword(loginUserDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user {Username}. Invalid username or password.", loginUserDto.Username);
                    return Unauthorized("Invalid username or password.");
                }

                _logger.LogInformation("User {Username} logged in successfully.", loginUserDto.Username);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in user {Username}.", loginUserDto.Username);
                return StatusCode(500, "An error occurred while logging in.");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        public static void CreatePasswordHash(string password, out string passwordHash)
        {
            passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
