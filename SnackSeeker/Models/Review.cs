using System;
using System.Collections.Generic;

namespace SnackSeeker
{
    public partial class Review
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public string RestaurantId { get; set; }
        public int? ReviewOfPrice { get; set; }
        public double? ReviewOfType1 { get; set; }
        public double? ReviewOfType2 { get; set; }
        public double? ReviewOfType3 { get; set; }
        public double? ReviewOfRating { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
