using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnackSeeker.Models
{
	public class ReviewModel
	{
		public RestaurantRoot Restaurant { get; set; }
		public Review UpdateRestaurantReview { get; set; }
	}
}