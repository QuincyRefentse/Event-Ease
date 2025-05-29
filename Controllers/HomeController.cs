using EventEase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchTerm, int? eventTypeId, int? venueId,
                                  DateTime? startDate, DateTime? endDate,
                                  bool? availableOnly, int page = 1)
        {
            int pageSize = 7;

            var eventsQuery = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .Include(e => e.EventType)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                eventsQuery = eventsQuery.Where(e =>
                    e.EventName.Contains(searchTerm) ||
                    e.Description.Contains(searchTerm) ||
                    e.Venue.VenueName.Contains(searchTerm));
            }

            if (eventTypeId.HasValue)
                eventsQuery = eventsQuery.Where(e => e.EventTypeId == eventTypeId.Value);

            if (venueId.HasValue)
                eventsQuery = eventsQuery.Where(e => e.VenueId == venueId.Value);

            if (startDate.HasValue)
                eventsQuery = eventsQuery.Where(e => e.EventDate >= startDate.Value);

            if (endDate.HasValue)
                eventsQuery = eventsQuery.Where(e => e.EventDate <= endDate.Value);

            if (availableOnly == true)
                eventsQuery = eventsQuery.Where(e => !e.Bookings.Any());

            // Prepare dropdown data
            ViewData["SearchTerm"] = searchTerm;
            ViewData["EventTypeId"] = eventTypeId;
            ViewData["VenueId"] = venueId;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["AvailableOnly"] = availableOnly ?? false;

            ViewData["EventTypes"] = new SelectList(_context.EventTypes.ToList(), "EventTypeId", "Name");
            ViewData["Venues"] = new SelectList(_context.Venues.ToList(), "VenueId", "VenueName");

            var pagedEvents = eventsQuery
                .OrderBy(e => e.EventDate)
                .ToPagedList(page, pageSize);

            return View(pagedEvents);
        }

        // Other actions remain same...
        // Privacy Page (optional)
        public IActionResult Privacy()
        {
            return View();
        }
    }

}

/*
using EventEase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
//using YourNamespace.Data; // Replace with your actual namespace
//using YourNamespace.Models;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string searchTerm, int? eventTypeId, int? venueId, DateTime? startDate, DateTime? endDate, bool? availableOnly, int page = 1)
    {
        int pageSize = 7;

        var eventsQuery = _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Bookings)
            .Include(e => e.EventType)
            .AsNoTracking()
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            eventsQuery = eventsQuery.Where(e =>
                e.EventName.Contains(searchTerm) ||
                e.Description.Contains(searchTerm) ||
                e.Venue.VenueName.Contains(searchTerm));
        }

        // Filter by Event Type
        if (eventTypeId.HasValue)
            eventsQuery = eventsQuery.Where(e => e.EventTypeId == eventTypeId.Value);

        // Filter by Venue
        if (venueId.HasValue)
            eventsQuery = eventsQuery.Where(e => e.VenueId == venueId.Value);

        // Filter by Date Range
        if (startDate.HasValue)
            eventsQuery = eventsQuery.Where(e => e.EventDate >= startDate.Value);

        if (endDate.HasValue)
            eventsQuery = eventsQuery.Where(e => e.EventDate <= endDate.Value);

        // Filter by Availability (no bookings)
        if (availableOnly == true)
            eventsQuery = eventsQuery.Where(e => !e.Bookings.Any());

        // Prepare dropdown data
        ViewData["SearchTerm"] = searchTerm;
        ViewData["EventTypeId"] = eventTypeId;
        ViewData["VenueId"] = venueId;
        ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
        ViewData["AvailableOnly"] = availableOnly ?? false;

        ViewData["EventTypes"] = new SelectList(_context.EventTypes.ToList(), "EventTypeId", "Name");
        ViewData["Venues"] = new SelectList(_context.Venues.ToList(), "VenueId", "VenueName");

        var pagedEvents = eventsQuery.OrderBy(e => e.EventDate).ToPagedList(page, pageSize);
        return View(pagedEvents);
    }

    // Privacy Page (optional)
    public IActionResult Privacy()
    {
        return View();
    }
}


*/



/*
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





*/


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