using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookMyShowClone.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name Field Cannot be null")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description Field Cannot be null")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Artist Field Cannot be null")]
        public string Artist { get; set; }

        [Required(ErrorMessage = "Language Field Cannot be null")]
        public string Language { get; set; }

        [Required(ErrorMessage = "City Field Cannot be null")]
        public string City { get; set; }

        public string Duration { get; set; }

        [Required(ErrorMessage = "PlayingDate Field Cannot be null")]
        public DateTime PlayingDate { get; set; }
        [Required(ErrorMessage = "PlayingTime Field Cannot be null")]
        public DateTime PlayingTime { get; set; }

        [Required(ErrorMessage = "TicketPrice Field Cannot be null")]
        public double TicketPrice { get; set; }

        [Required(ErrorMessage = "UnReservedSeats Field Cannot be null")]
        public int UnReservedSeats { get; set; }

        public int ReservedSeats { get; set; } = 0;

        public double Rating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;

        public string Genre { get; set; }
        public string TrailorUrl { get; set; }

        //public int MyProperty { get; set; }

        [NotMapped]
        public IFormFile Image { get; set; }

        public string ImageUrl { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
    }
}
