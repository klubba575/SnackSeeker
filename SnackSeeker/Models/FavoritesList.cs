using System;
using System.Collections.Generic;

namespace SnackSeeker.Models
{
    public partial class FavoritesList
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantId { get; set; }
        public string Price { get; set; }
        public double? Rating { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
