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
        //Dependency Injection variable Instantiation
        private readonly string _yelpKey;
        private readonly HttpClient _client;
        private readonly SnacksDbContext _context;

        //Constructor for Dependency Injection
        public FavoritesController(IConfiguration configuration, SnacksDbContext context)
        {
            _yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);
            _context = context;
        }

        //Displays the Logged-in user's favorites
        public IActionResult DisplayFavorites()
        {
            //gets the user ID for the currently logged in user
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            //Pull the favorites from the FavoriteList Table and then sort the list, taking only those that are owned by the currently logged in user
            var favorites = _context.FavoritesList.Where(x => x.UserId == id).ToList();
            //return the favorites to the view
            return View(favorites);
        }

        //Method to add a favorite to the logged-in user's list
        //Id is the restaurant's Id in the Yelp API, string name is the restaurant to be added and string price is the Yelp price rating
        public IActionResult AddToFavorites(string Id, string name, string price)
        {
            //Get the currently logged-in user userID
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