using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SnackSeeker.Models;

namespace SnackSeeker.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly string _yelpKey;
        private readonly HttpClient _client;
        private readonly SnacksDbContext _context;

        public FavoritesController(IConfiguration configuration, SnacksDbContext context)
        {
            _yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);
            _context = context;
        }

        public IActionResult DisplayFavorites()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var favorites = _context.FavoritesList.Where(x => x.UserId == id).ToList();

            return View(favorites);
        }
        public IActionResult AddToFavorites(string Id, string name, string price)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var reviews = _context.Review.Where(x => x.UserId == id).ToList();

            Review favRestaurant = new Review();
            foreach (var review in reviews)
            {
                if (Id == review.RestaurantId)
                {
                    favRestaurant = review;
                }
            }

            FavoritesList favorites = new FavoritesList
            {
                RestaurantName = name,
                Price = price,
                Rating = favRestaurant.ReviewOfRating,
                UserId = id
            };

            _context.FavoritesList.Add(favorites);
            _context.SaveChanges();
            return RedirectToAction("DisplayFavorites");
        }

        public IActionResult RemoveFavorite(int id)
        {
            var foundFav = _context.FavoritesList.Find(id);
            _context.FavoritesList.Remove(foundFav);
            _context.SaveChanges();

            return RedirectToAction("DisplayFavorites");
        }
    }
}