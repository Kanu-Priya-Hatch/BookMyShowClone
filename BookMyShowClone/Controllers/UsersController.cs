using AuthenticationPlugin;
using BookMyShowClone.Data;
using BookMyShowClone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace BookMyShowClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EventDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly AuthService _auth;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(EventDbContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._dbContext = dbContext;
            this._configuration = configuration;
            _auth = new AuthService(configuration);
            this._httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] User user)
        {
            var userWithSameEmail = _dbContext.Users.Where(u => u.Email == user.Email).SingleOrDefault();

            var email = user.Email;

            Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                                        RegexOptions.CultureInvariant | RegexOptions.Singleline);

            bool isValidEmail = regex.IsMatch(email);

            if (userWithSameEmail != null)
            {
                return BadRequest("User already exists with same email");
            }
            else if (!isValidEmail)
            {
                return BadRequest("The email is invalid");

            }
            else
            {
                var userObj = new User
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = SecurePasswordHasherHelper.Hash(user.Password),

                    Role = (user.Role != null) ? user.Role : "Users"

                };
                _dbContext.Users.Add(userObj);
                _dbContext.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        [Authorize(Roles = "Users")]
        [HttpGet("CheckFavoriteEvents")]
        public IActionResult CheckFavoriteEvents()
        {
            var userId = GetUserId();
            var favorites = from favorite in _dbContext.Favorites
                            join movie in _dbContext.Events on favorite.EventId equals movie.Id
                            join user in _dbContext.Users on favorite.UserId equals user.Id
                            where (favorite.UserId == userId)
                            select new
                            {
                                EventName = movie.Name,
                                ArtistName = movie.Artist,
                                City = movie.City
                            };

            return Ok(favorites);
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] User user)
        {
            var userEmail = _dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userEmail == null)
            {
                return NotFound();
            }

            if (!SecurePasswordHasherHelper.Verify(user.Password, userEmail.Password))
            {
                return Unauthorized();
            }

            var claims = new[]
                 {

                        new Claim(ClaimTypes.NameIdentifier, userEmail.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, userEmail.Role),

                  };

            var token = _auth.GenerateAccessToken(claims);

            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn,
                token_type = token.TokenType,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
                user_id = userEmail.Id,

            });

        }


        [Authorize(Roles = "Users")]
        [HttpPost("AddFavorite/{id}")]
        public IActionResult AddFavorite(int id)
        {

            var favObj = new Favorite
            {
                EventId = id,
                UserId = GetUserId()
            };

            _dbContext.Favorites.Add(favObj);
            _dbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);

        }

        [Authorize(Roles = "Users")]
        [HttpPost("RateEvent/{id}")]
        public IActionResult RateEvent(int id, [FromForm] double rating)
        {
            var events = _dbContext.Events.Find(id);

            if (events == null)
            {
                return NotFound("No record found with this id");
            }

            else
            {
                var prevCount = events.RatingCount;
                var prevRating = events.Rating;
                var newRating = (prevCount * prevRating + rating) / (prevCount + 1);
                events.Rating = newRating;
                events.RatingCount++;
                _dbContext.SaveChanges();
                return Ok("Record updated successfully");

            }



        }
    }
}