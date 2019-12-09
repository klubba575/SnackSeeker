using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SnackSeeker.Controllers
{
    public class SnackController : Controller
    {
        public IActionResult PreferenceIndex()
        {
            return View();
        }

        //TODO: Make a model for preference history including a List of Review and Categories
        public IActionResult PreferenceHistory()
        {
            return View();
        }
    }
}