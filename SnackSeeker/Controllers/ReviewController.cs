using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SnackSeeker.Models;

namespace SnackSeeker.Controllers
{
	public class ReviewController : Controller
	{

		private readonly string _yelpKey;
		private readonly HttpClient _client;
		private readonly SnacksDbContext _context;


		public ReviewController(IConfiguration configuration, SnacksDbContext context)
		{
			_yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
			_client = new HttpClient();
			_client.BaseAddress = new Uri("https://api.yelp.com/v3/");
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);
			_context = context;
		}
		public async Task<IActionResult> ReviewCheck(string id)
		{
			string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			var reviews = _context.Review.Where(x => x.UserId == id).ToList();
			foreach (var review in reviews)
			{
				if(review.RestaurantId == id)
				{
					return View("ReviewChange", review);
				}
			}
			
			var reviewResponse = await _client.GetAsync($"businesses/{id}");
			var results = await reviewResponse.Content.ReadAsAsync<RestaurantRoot>();

			return View("ReviewAdd" );
		}
		

	}
	
}
		
