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
    public class SnackController : Controller
    {
        private readonly ILogger<SnackController> _logger;
        private readonly Location _yelpLocationContext;
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
            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult SearchCategory()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SearchCategory(string tag)
        {
            // check to make sure the user has 3 preferences saved. If not send them to a view to add preferences.

            //int topFirst = 0;
            //int topSecond = 0;
            //int topThird = 0;

            var categories = _context.Preferences.ToList();
            //for (int i = 0; i < categories.Count; i++)
            //{
            //    if(categories[i].Rating > categories[topFirst].Rating)
            //    {
            //        topFirst = i;
            //    }
            //}
            //categories[topFirst].Rating = -1;
            
            //for (int j = 0; j < categories.Count; j++)
            //{

            //    if (categories[j].Rating > categories[topSecond].Rating)
            //    {
            //        topSecond = j;
            //    }
            //}
            //categories[topSecond].Rating = -1;

            //for (int k = 0; k < categories.Count; k++)
            //{
            //    if (categories[k].Rating > categories[topThird].Rating)
            //    {
            //        topThird= k;
            //    }
            //}
            //categories[topThird].Rating = -1;

            var sortedList = categories.OrderByDescending(x => x.Rating).ToList();


            var tagResponse = await _client.GetAsync($"businesses/search?location={tag}&categories={sortedList[0].Name},{sortedList[1].Name},{sortedList[2].Name}");
            var tagResults = await tagResponse.Content.ReadAsAsync<BusinessRoot>();

            return View("ListCategory",tagResults);
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
    }
}