using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList;
using X.PagedList.Extensions;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Main Index Action with Search and Pagination
        public IActionResult Index(string searchTerm, int page = 1)
        {
            int pageSize = 7; // Number of events per page

            var eventsQuery = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .AsNoTracking() // Optional: improve performance for read-only ops
                .AsQueryable();

            // Apply filtering if search term is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                eventsQuery = eventsQuery.Where(e =>
                    e.EventName.Contains(searchTerm) ||
                    e.Description.Contains(searchTerm) ||
                    e.Venue.VenueName.Contains(searchTerm) ||
                    e.Venue.VenueLocaiton.Contains(searchTerm));
            }

            ViewData["SearchTerm"] = searchTerm;

            // Pagination
            var pagedEvents = eventsQuery
                .OrderBy(e => e.EventDate)
                .ToPagedList(page, pageSize);

            return View(pagedEvents);
        }

        // Privacy Page (optional)
        public IActionResult Privacy()
        {
            return View();
        }

        // Error Handler
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}






/*
using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using Carbon.PagedList;  // Add this for the Carbon.PagedList functionality

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Database context

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .ToList();

            return View(events); // Passing event list to the view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


*/

/*using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
*/