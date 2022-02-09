using BookMyShowClone.Data;
using BookMyShowClone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookMyShowClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly EventDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReservationController(EventDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this._dbContext = dbContext;
            this._httpContextAccessor = httpContextAccessor;
        }
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));


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

        [Authorize(Roles = "Users")]
        [HttpGet("CheckPreviousBooking")]
        public IActionResult CheckPreviousBooking()
        {
            var reservations = from reservation in _dbContext.Reservations
                               join customer in _dbContext.Users on reservation.UserId equals customer.Id
                               join movie in _dbContext.Events on reservation.EventId equals movie.Id
                               where (customer.Id == GetUserId())
                               select new
                               {
                                   Id = reservation.Id,
                                   ReservationTime = reservation.ReservationTime,
                                   CustomerName = customer.Name,
                                   EventName = movie.Name
                               };

            return Ok(reservations);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("TotalEarningSingleEvent/{id}")]
        public IActionResult TotalEarningSingleEvent(int id)
        {
            var reservations = from reservation in _dbContext.Reservations
                               where (reservation.EventId == id)
                               select new
                               {
                                   TotalAmount = reservation.TotalAmount                                  
                               };
            double result = 0;
            foreach (var reservation in reservations)
            {
                result += reservation.TotalAmount;               
            }

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("TotalEarningAllEvent")]
        public IActionResult TotalEarningAllEvent()
        {
            var reservations = from reservation in _dbContext.Reservations
                               select new
                               {                                 
                                    TotalAmount = reservation.TotalAmount                               
                               };

            double result = 0;
            foreach (var reservation in reservations)
            {
                result += reservation.TotalAmount;
            }

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("TotalBookingEvent/{id}")]
        public IActionResult TotalBookingEvent(int id)
        {
            var reservations = from reservation in _dbContext.Reservations
                               where (reservation.EventId == id)
                               select new
                               {
                                   Qty = reservation.Qty                                   
                               };

            int bookCount = 0;
            foreach (var reservation in reservations)
            {
                bookCount += reservation.Qty;
            }

            return Ok(bookCount);

        }
               

    }
}
