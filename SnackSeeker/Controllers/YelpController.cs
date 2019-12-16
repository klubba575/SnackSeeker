using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SnackSeeker.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SnackSeeker.Controllers
{
    public class YelpController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _yelpKey;

        public YelpController(IHttpClientFactory client, IConfiguration configuration)
        {
            _yelpKey = configuration.GetSection("ApiKeys")["YelpApi"];
            _client = client.CreateClient();
            _client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yelpKey);

        }
    }
}