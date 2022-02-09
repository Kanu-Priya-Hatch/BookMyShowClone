using BookMyShowClone.Data;
using BookMyShowClone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookMyShowClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly EventDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventsController(EventDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this._dbContext = dbContext;
            this._httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));


        [HttpPost("AddFavorite/{id}")]
        [Authorize(Roles = "Users")]
        public IActionResult Post(int id)
        {
            
            var favObj = new Favorite
            {
                EventId = id,
                UserId = GetUserId()
            };

            if (favObj == null)
            {
                return NotFound("No record found with this id");
            }
            else
            {
                _dbContext.Favorites.Add(favObj);
                _dbContext.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }

        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromForm] Event eventObj)
        {
            var events = _dbContext.Events.Find(id);

            if (events == null)
            {
                return NotFound("No record found with this id");
            }

            else
            {
                var guid = Guid.NewGuid();
                var filepath = Path.Combine("wwwroot", guid + ".jpg");

                if (events.Image != null)
                {
                    var fileStream = new FileStream(filepath, FileMode.Create);
                    events.Image.CopyTo(fileStream);
                    events.ImageUrl = filepath.Remove(0, 7);

                }

                events.Name = eventObj.Name;
                events.Language = eventObj.Language;
                events.PlayingDate = eventObj.PlayingDate;
                events.PlayingTime = eventObj.PlayingTime;
                events.Rating = eventObj.Rating;
                events.TicketPrice = eventObj.TicketPrice;
                events.TrailorUrl = eventObj.TrailorUrl;
                events.Genre = eventObj.Genre;
                events.Duration = eventObj.Duration;
                events.Description = eventObj.Description;
                events.Artist = eventObj.Artist;

                _dbContext.SaveChanges();
                return Ok("Record updated successfully");


            }


        }

      
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var events = _dbContext.Events.Find(id);

            if (events == null)
            {
                return NotFound("No record found with this id");
            }

            else
            {
                _dbContext.Events.Remove(events);
                _dbContext.SaveChanges();
                return Ok("Record deleted successfully");
            }

        }

        [Authorize]
        [HttpGet("{info}")]
        public IActionResult FindMovies(string info)
        {
            var movies = from movie in _dbContext.Events
                         where movie.City.StartsWith(info) ||
                         movie.Name.StartsWith(info) ||
                         movie.Artist.StartsWith(info) || 
                         movie.Genre.StartsWith(info) 
                         select new
                         {
                             Id = movie.Id,
                             Name = movie.Name,
                             ImageUrl = movie.ImageUrl
                         };
            return Ok(movies);
        }

        [HttpPost]
        [Authorize(Roles = "Users")]
        public IActionResult Post([FromBody] Reservation reservationObj)
        {
            reservationObj.ReservationTime = DateTime.Now;
            reservationObj.UserId = GetUserId();
            reservationObj.Price = _dbContext.Events.Where(m => m.Id == reservationObj.EventId)
                                    .Select(m => m.TicketPrice).SingleOrDefault();

            reservationObj.TotalAmount = reservationObj.Price * reservationObj.Qty;
            _dbContext.Reservations.Add(reservationObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
