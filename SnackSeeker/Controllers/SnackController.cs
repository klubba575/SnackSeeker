using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnackSeeker.Models;

namespace SnackSeeker.Controllers
{
    public class SnackController : Controller
    {
        private readonly ILogger<SnackController> _logger;
        private readonly Location _yelpLocationContext;
        private readonly string _yelpKey;
        private readonly HttpClient _client;

        public SnackController(ILogger<SnackController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }

        public async Task<IActionResult> Search(string tag)
        {
            var tagResponse = await _client.GetAsync($"categories/{tag}");
            var tagResults = await tagResponse.Content.ReadAsAsync<Categories>();

            return View(tagResults);
        }
    }
}