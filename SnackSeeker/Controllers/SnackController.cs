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
        private readonly ILogger<SnackController> _logger;
        private readonly string _yelpKey;
        private readonly HttpClient _client;
        private readonly SnacksDbContext _context;
        private readonly double snackAlgo;

        public SnackController(ILogger<SnackController> logger, IConfiguration configuration, SnacksDbContext context)
        {
            _logger = logger;
            _yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);
            _context = context;
        }
        public IActionResult PreferenceIndex()
        {
            var preferences = _context.Preferences.ToList();
            var userPreferences = new List<Preferences>();
            foreach (var pref in preferences)
            {
                if (pref.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    userPreferences.Add(pref);
                }
            }
            return View(userPreferences);
        }
        [HttpGet]
        public IActionResult SearchCategory()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SearchCategory(string tag, string Price1, string Price2, string Price3, string Price4)
        {
			List<string> checkPrice = new List<string>() { Price1, Price2, Price3, Price4 };
			
			var multiplePrices = "";
			foreach (var price in checkPrice)
			{
				if (price != null)
				{
					multiplePrices += $"{price},";
				}
			}
			multiplePrices = multiplePrices.TrimEnd(',');


			string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var categories = _context.Preferences.Where(x => x.UserId == id).ToList();

            var sortedList = categories.OrderByDescending(x => x.Rating).ToList();
			HttpResponseMessage tagResponse;
			if (multiplePrices.Length == 0)
			{
				tagResponse = await _client.GetAsync($"businesses/search?location={tag}&categories={sortedList[0].Name},{sortedList[1].Name},{sortedList[2].Name}");
			}
			else
			{
				tagResponse = await _client.GetAsync($"businesses/search?location={tag}&categories={sortedList[0].Name},{sortedList[1].Name},{sortedList[2].Name}&price={multiplePrices}");
			}			
			var tagResults = await tagResponse.Content.ReadAsAsync<BusinessRoot>();
            return View("ListCategory", tagResults);
        }

        public IActionResult ListCategory(BusinessRoot result)
        {
            return View(result);
        }

        //TODO: Make a model for preference history including a List of Review and Categories
        public IActionResult PreferenceHistory()
        {
            return View();
        }

        public IActionResult UserPreference(string weight)
        {
            var findPreference = _context.Preferences.Find("UserPreference");
            if(findPreference == null)
            {
                Preferences userPreference = new Preferences();
                userPreference.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                userPreference.Name = "UserPreference";
                userPreference.Rating = double.Parse(weight);
                _context.Preferences.Add(userPreference);
            }
            else
            {
                findPreference.Rating = double.Parse(weight);
                _context.Entry(findPreference).State = EntityState.Modified;
                _context.Update(findPreference);
            }
            _context.SaveChanges();
            
            // Rating
            if(weight == "1.0")
            {
                
            }

            // Type
            else if (weight == "2.0")
            {

            }
            // Price
            else
            {

            }
            return RedirectToAction("PreferenceIndex");
        }

        public IActionResult PreferenceAdd(string category, int rating)
        {
            Preferences newPreferences = new Preferences();
            newPreferences.Name = category;
            newPreferences.Rating = rating;
            newPreferences.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var findCategory = _context.Preferences.ToList();
            _context.Preferences.Add(newPreferences);
            _context.SaveChanges();
            return RedirectToAction("PreferenceIndex");
        }

        public IActionResult PreferenceChange(string change, int id, int rating)
        {
            var pref = _context.Preferences.Find(id);
            if (change == "Delete")
            {
                _context.Preferences.Remove(pref);
            }
            else if (change == "Update")
            {
                pref.Rating = rating;
                _context.Entry(pref).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.Preferences.Update(pref);
            }
            _context.SaveChanges();
            return RedirectToAction("PreferenceIndex");
        }
    }
}