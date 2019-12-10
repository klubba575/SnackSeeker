using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SnackSeeker.Models;
using Microsoft.EntityFrameworkCore;

namespace SnackSeeker.Controllers
{
    public class SnackController : Controller
    {
        private readonly SnacksDbContext _context;
        private readonly double snackAlgo;

        public SnackController(SnacksDbContext context)
        {
            _context = context;
        }
        public IActionResult PreferenceIndex()
        {
            return View();
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