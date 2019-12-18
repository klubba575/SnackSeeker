using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnackSeeker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SnackSeeker.Controllers
{
	[Authorize]
    public class SnackController : Controller
    {
		//Initializing our variables to be used for API and database interaction
        private readonly string _yelpKey;
        private readonly HttpClient _client;
        private readonly SnacksDbContext _context;

		
        public SnackController(IConfiguration configuration, SnacksDbContext context)
        {
			//Dependancy injecting our snacks controller
            _yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
            _client = new HttpClient();
			//Setting up the Base address and necessary header for our Yelp API calls
            _client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);
            _context = context;
        }
		//Private method that records the average of a user's price reviews from all their reviews in the database
		//This creates the price average variable to guide the user towards their own usual preference
		private void CalcAverage()
		{
			//Takes all the reviews in the database and puts them into a list
			var reviews = _context.Review.ToList();
			double priceAverage = 0;
			int counter = 0;
			foreach (var review in reviews) 
			{
				if(review.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
				{
					priceAverage = priceAverage + (double)(review.ReviewOfPrice);
					counter++;
				}
			}
			priceAverage = priceAverage / counter;
			//If the user does not have any reviews, this creates a default price average of 1
			if (double.IsNaN(priceAverage))
			{
				priceAverage = 1;
			}
			var id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			var user = _context.AspNetUsers.Where(x => x.Id == id).First();

			user.PriceAverage = priceAverage;
			_context.Entry(user).State = EntityState.Modified;
			_context.SaveChanges();
		}

        //Method That displays the user's preferences and weights
        //Calculate the Average Price to ensure that it is always as up to date as possible.
		//Home Page for our Site, shows all our User's Preferences
        public IActionResult PreferenceIndex(string random)
        {
			//Method to show the logged in user's average price based on reviews
            CalcAverage();

            var userAve = _context.AspNetUsers.ToList();
            double? userAverage;
            var id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _context.AspNetUsers.Where(x => x.Id == id).ToList();
            userAverage = user[0].PriceAverage;
            
			ViewBag.Average = userAverage;
			ViewBag.highcategory = TempData["highcategory"];

			//Takes the user's preferences from the database and displays them as a List
            var userPreferences = new List<Preferences>();
            if(random != null) 
            {
                userPreferences = RandomizeThreeTimes();
                foreach(var ranPref in userPreferences)
                {
                    AddPreference(ranPref.Name, (int)ranPref.Rating);
                }
            }

            return View(_context.Preferences.Where(x => x.UserId == id).ToList());
        }
		//Method that adds preferences to each user's preferences in the database
        public IActionResult PreferenceAdd(string category, int rating)
        {
            AddPreference(category, rating);
            return RedirectToAction("PreferenceIndex");
        }

        public void AddPreference(string category, int rating)
        {
            Preferences newPreferences = new Preferences();
            newPreferences.Name = category;
            newPreferences.Rating = rating;
            newPreferences.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _context.Preferences.Add(newPreferences);
            _context.SaveChanges();
        }
		//Method that allows you to update or delete your preferences
		//Both options change that user's data in the database and redisplays the Preference Index accordingly

       public IActionResult PreferenceChange(string change, int id, int rating)
        {
			//Delete a user preference
            var pref = _context.Preferences.Find(id);
            if (change == "Delete")
            {
                _context.Preferences.Remove(pref);
            }
			//Update the rating on a user preference
            else if (change == "Update")
            {
                pref.Rating = rating;
                _context.Entry(pref).State = EntityState.Modified;
                _context.Preferences.Update(pref);
            }
            _context.SaveChanges();
            return RedirectToAction("PreferenceIndex");
        }
		[HttpGet]
		//Get Method that is used for getting user search filters
		public IActionResult SearchCategory()
		{
			CalcAverage();
			var userAve = _context.AspNetUsers.ToList();
			double? userAverage;
			var userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			var user = _context.AspNetUsers.Where(x => x.Id == userid).First();
			userAverage = user.PriceAverage;
			ViewBag.Average = userAverage;
			return View();
		}

        //method which displays a person
        [HttpPost]
		//Post Method that uses user filter along with the Yelp API to bring up snack results based on the filters
        public async Task<IActionResult> SearchCategory(string tag, string Price1, string Price2, string Price3, string Price4, string random, string sortBy)
        {

            List<string> checkPrice = new List<string>() { Price1, Price2, Price3, Price4 };
			
			//Option to search for more than one price point
            var multiplePrices = "";
            foreach (var price in checkPrice)
            {
                if (price != null)
                {
                    multiplePrices += $"{price},";
                }
            }
            multiplePrices = multiplePrices.TrimEnd(',');

			//Using logged in user's preference List to search
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var categories = _context.Preferences.Where(x => x.UserId == id).ToList();
            List<Preferences> preferences = new List<Preferences>();
            HttpResponseMessage tagResponse;

            //Using random, price, alphabetized, or rating preferences to filter our search
            if (random == "RandomTotal")
            {
                preferences = RandomizeThreeTimes();
            }
            else if (random == "RandomPersonal")
            {
                preferences = ThreeRandomFavorites();
            }
            else
            {
                preferences = categories.OrderByDescending(x => x.Rating).ToList();
            }

            if (multiplePrices.Length == 0)
            {
                tagResponse = await _client.GetAsync($"businesses/search?location={tag}&categories={preferences[0].Name},{preferences[1].Name},{preferences[2].Name}");
            }
            else
            {
                tagResponse = await _client.GetAsync($"businesses/search?location={tag}&categories={preferences[0].Name},{preferences[1].Name},{preferences[2].Name}&price={multiplePrices}");
            }
            var tagResults = await tagResponse.Content.ReadAsAsync<BusinessRoot>();

            if (sortBy == "Name")
            {
                tagResults.businesses.Sort((x, y) => string.Compare(x.name, y.name));
            }
            else if (sortBy == "NameReverse")
            {
                tagResults.businesses.Sort((x, y) => string.Compare(x.name, y.name));
                tagResults.businesses.Reverse();
            }
            else if(sortBy == "Price")
            {
                tagResults.businesses.Sort((x, y) => string.Compare(x.price, y.price));
            }
            else if (sortBy == "PriceReverse")
            {
                tagResults.businesses.Sort((x, y) => string.Compare(x.price, y.price));
                tagResults.businesses.Reverse();

            }
            else if (sortBy == "Rating")
            {
                tagResults.businesses.Sort((x, y) => x.rating.CompareTo(y.rating));
            }
            else if(sortBy == "RatingReverse")
            {
                tagResults.businesses.Sort((x, y) => x.rating.CompareTo(y.rating));
                tagResults.businesses.Reverse();
            }
            return View("ListCategory", tagResults);
        }
		
		//Method to List out all our restuls
        public IActionResult ListCategory(BusinessRoot result)
		{
			return View(result);
		}
		//Method used to randomize all the different options in our Categories class
        public string RandomizeAll()
        {
            var categories = Categories.Category;
            var rand = new Random();
            int selected = rand.Next(categories.Count);
            return categories[selected];
        }
		//Method used to give three random categories for a new user if they do not wish to choose their own
		//Each random preference created is assigned to the logged in user and given a rating of three
        public List<Preferences> RandomizeThreeTimes()
        {
            List<string> categories = new List<string>();
            string newCategory = "";
            while (categories.Count != 3)
            {
                newCategory = RandomizeAll();
                if (!categories.Contains(newCategory))
                {
                    categories.Add(newCategory);
                }
            }
            List<Preferences> prefs = new List<Preferences>();
            Preferences newPreference1 = new Preferences();
            Preferences newPreference2 = new Preferences();
            Preferences newPreference3 = new Preferences();
            newPreference1.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            newPreference1.Rating = 3;
            newPreference1.Name = categories[0];
            newPreference2.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            newPreference2.Rating = 3;
            newPreference2.Name = categories[1];
            newPreference3.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            newPreference3.Rating = 3;
            newPreference3.Name = categories[2];
            prefs.Add(newPreference1);
            prefs.Add(newPreference2);
            prefs.Add(newPreference3);

            return prefs;
        }

        //Method pulls three random preferences from the user's already established list of preferences
        public List<Preferences> ThreeRandomFavorites()
        {
            List<Preferences> favPrefs = new List<Preferences>();
            var establishedPreferences = _context.Preferences.ToList();
            Random rand = new Random();
            var id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            int find;
            while (favPrefs.Count != 3)
            {
                find = rand.Next(0, establishedPreferences.Count);
                if(!favPrefs.Contains(establishedPreferences[find]) && establishedPreferences[find].Rating >= 3)
                {
                    favPrefs.Add(establishedPreferences[find]);
                }
            }

            return favPrefs;
        }
	}
}