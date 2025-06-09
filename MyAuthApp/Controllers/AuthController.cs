using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyAuthApp.Data;
using MyAuthApp.Dtos;       // ← Make sure this line is present
using MyAuthApp.Models;

namespace MyAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        // GET /api/auth/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "pong" });
        }

        // POST /api/auth/signup
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserRegisterDto dto)
        {
            // 1. Make sure username and email are unique
            if (await _dbContext.Users.AnyAsync(u => u.Username == dto.Username))
                return Conflict(new { message = "Username already taken." });

            if (await _dbContext.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { message = "Email already registered." });

            // 2. Create new User entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Hash the password
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            // 4. Save to SQLite
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "User created successfully." });
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            // 1. Look up by username
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            // 2. Verify password
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid credentials." });

            // 3. Build JWT
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Key"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("JWT Key is not configured.");

            // Convert the secretKey string to a byte[] before passing to SymmetricSecurityKey:
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                              double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")
                          ),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = jwtString,
                expiresIn = tokenDescriptor.Expires
            });
        }

                [Authorize]
        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] UserUpdateDto dto)
        {
            // 1. Get current user ID from JWT "sub" claim
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(sub, out var userId))
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // 2. Update username/email if supplied (and unique)
            if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
            {
                if (await _dbContext.Users.AnyAsync(u => u.Username == dto.Username))
                    return Conflict(new { message = "Username already taken." });
                user.Username = dto.Username;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _dbContext.Users.AnyAsync(u => u.Email == dto.Email))
                    return Conflict(new { message = "Email already registered." });
                user.Email = dto.Email;
            }

            // 3. Update password if supplied
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            }

            // 4. Persist changes
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Account updated successfully." });
        }

        // ===============
        // DELETE ACCOUNT
        // ===============
        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(sub, out var userId))
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Account deleted successfully." });
        }
    }
}

    }
}
