using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnackSeeker.Models;

namespace SnackSeeker.Controllers
{
    [Authorize]
    public class PreferenceController : Controller
    {
        private readonly ILogger<PreferenceController> _logger;
        private readonly string _yelpKey;
        private readonly HttpClient _client;
        private readonly SnacksDbContext _context;
        private readonly double snackAlgo;

        public PreferenceController(ILogger<PreferenceController> logger, IConfiguration configuration, SnacksDbContext context)
        {
            _logger = logger;
            _yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);
            _context = context;
        }

    }
}