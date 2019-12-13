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
			var reviews = _context.Review.ToList();
			var reviewResponse = await _client.GetAsync($"businesses/{id}");
			var results = await reviewResponse.Content.ReadAsAsync<RestaurantRoot>();
			foreach (var review in reviews)
			{
				if (review.RestaurantId == id)
				{
					ReviewModel updateReview = new ReviewModel();
					updateReview.Restaurant = results;
					updateReview.UpdateRestaurantReview = review;
					return View("ReviewChange", updateReview);
				}
			}


			return View("ReviewAdd", results);
		}
		public IActionResult ReviewChange(string change, string restaurantId, int reviewOfPrice, double reviewOfType1, double reviewOfType2, double reviewOfType3, double reviewOfRating, string restaurantName)
		{
			string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			var reviewList = _context.Review.Where(x => x.UserId == userId).ToList();
			Review review = new Review();
			foreach (var currentReview in reviewList)
			{
				if (currentReview.RestaurantId == restaurantId)
				{
					review = currentReview;

				}
			}

			if (change == "Delete")
			{
				_context.Review.Remove(review);
			}
			else if (change == "Update")
			{
				review.RestaurantName = restaurantName;
				review.RestaurantId = restaurantId;
				review.UserId = userId;
				review.ReviewOfPrice = reviewOfPrice;
				review.ReviewOfRating = reviewOfRating;
				review.ReviewOfType1 = reviewOfType1;
				review.ReviewOfType2 = reviewOfType2;
				review.ReviewOfType3 = reviewOfType3;
				_context.Entry(review).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				_context.Review.Update(review);
			}
			_context.SaveChanges();
			return RedirectToAction("PreferenceIndex", "Snack");

		}
		public IActionResult ReviewAdd(Review newReview)
		{
			if (ModelState.IsValid)
			{
				_context.Review.Add(newReview);
				_context.SaveChanges();
				return RedirectToAction("PreferenceIndex", "Snack");
			}
			return RedirectToAction("ReviewCheck", newReview.RestaurantId);
		}
		public IActionResult FindHighestCategory()
		{
			List<string> typeOneList = new List<string>();
			List<string> typeTwoList = new List<string>();
			List<string> typeThreeList = new List<string>();

			string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			typeOneList = _context.Review.Where(x => x.UserId == userId).Select(x => x.Type1Name).ToList();
			typeTwoList = _context.Review.Where(x => x.UserId == userId).Select(x => x.Type2Name).ToList();
			typeThreeList = _context.Review.Where(x => x.UserId == userId).Select(x => x.Type3Name).ToList();
			
			typeOneList.AddRange(typeTwoList);
			typeOneList.AddRange(typeThreeList);
			Dictionary<string, int> reviewDictionary = new Dictionary<string, int>();
			foreach (string category in typeOneList)
			{
				if (reviewDictionary.ContainsKey(category))
				{
					reviewDictionary[category]++;
				}
				else
				{
					reviewDictionary.Add(category, 1);
				}
			}

			int biggest = reviewDictionary.Values.Max();
			List<string> highestReviewCategories = new List<string>();
			foreach (KeyValuePair<string, int> kvp in reviewDictionary)
			{
				if (biggest == kvp.Value)
				{
					highestReviewCategories.Add(kvp.Key);
				}
			}
			return View(highestReviewCategories);
		}
		public IActionResult ListReviews()
		{
			string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			var reviews = _context.Review.Where(x => x.UserId == userId).ToList();
			return View(reviews);
		}
	}
}

