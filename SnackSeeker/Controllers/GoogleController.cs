using System;
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

