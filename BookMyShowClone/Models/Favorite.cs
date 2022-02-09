using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookMyShowClone.Models
{
    public class Favorite
    {
        public int id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
    }
}
