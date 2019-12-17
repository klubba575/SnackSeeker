﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SnackSeeker.Controllers
{
    public class GoogleController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _googleKey;

        public GoogleController(IHttpClientFactory client, IConfiguration configuration)
        {
            _googleKey = configuration.GetSection("ApiKeys")["GoogleApi"];
            _client = client.CreateClient();
            _client.BaseAddress = new Uri("https://www.google.com/maps/embed/v1/");
        }
		//Method to display Imbedded map from Google API including the location from the user
		//and if applicable, the business name.
        [HttpGet]
        public IActionResult DisplayInfo()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DisplayInfo(string location, string name)
        {
            string both = $"{name}, {location}";
            if (name == null)
            {
                both = $"{location}";
            }

            ViewData["hidden"] = _googleKey;
            return View((object)both);
        }
    }
}


