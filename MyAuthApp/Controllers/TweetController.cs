using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAuthApp.Data;
using MyAuthApp.Dtos;
using MyAuthApp.Models;
using System.IdentityModel.Tokens.Jwt;


namespace MyAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TweetController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TweetController(ApplicationDbContext db) => _db = db;

        // ---------- Create ----------
        // POST api/tweet
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TweetCreateDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var tweet = new Tweet
            {
                Id        = Guid.NewGuid(),
                UserId    = userId.Value,
                Content   = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tweets.Add(tweet);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tweet.Id }, tweet);
        }

        // ---------- Read ----------
        // GET api/tweet/{id}
        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tweet = await _db.Tweets
                                 .Include(t => t.Author)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(t => t.Id == id);

            return tweet == null ? NotFound() : Ok(tweet);
        }

        // GET api/tweet
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tweets = await _db.Tweets
                                  .Include(t => t.Author)
                                  .OrderByDescending(t => t.CreatedAt)
                                  .AsNoTracking()
                                  .ToListAsync();
            return Ok(tweets);
        }

        // ---------- Update ----------
        // PUT api/tweet/{id}
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TweetUpdateDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var tweet = await _db.Tweets.FindAsync(id);
            if (tweet == null) return NotFound();
            if (tweet.UserId != userId) return Forbid();

            tweet.Content   = dto.Content;
            tweet.UpdatedAt = DateTime.UtcNow;

            _db.Tweets.Update(tweet);
            await _db.SaveChangesAsync();
            return Ok(tweet);
        }

        // ---------- Delete ----------
        // DELETE api/tweet/{id}
        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var tweet = await _db.Tweets.FindAsync(id);
            if (tweet == null) return NotFound();
            if (tweet.UserId != userId) return Forbid();

            _db.Tweets.Remove(tweet);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Tweet deleted." });
        }

        // ---------- Helpers ----------
        private Guid? GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) // if you mapped it
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(sub, out var id) ? id : (Guid?)null;
        }
    }
}
