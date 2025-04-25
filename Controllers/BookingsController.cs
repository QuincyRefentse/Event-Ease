using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            // Populate dropdown lists with Event and Venue data
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            // Basic validation for required fields
            if (booking.EventId == null || booking.VenueId == null)
            {
                ModelState.AddModelError(string.Empty, "Event and Venue are required.");
            }

            // Check for double booking only if we have necessary data
            if (booking.VenueId != null && booking.BookingDate != default)
            {
                var existingBooking = await _context.Bookings
                    .FirstOrDefaultAsync(b =>
                        b.VenueId == booking.VenueId &&
                        b.BookingDate.Date == booking.BookingDate.Date);

                if (existingBooking != null)
                {
                    ModelState.AddModelError("BookingDate", "This venue is already booked on the selected date.");
                }
            }

            // ✅ Only save if ALL validations pass
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }




        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Populate dropdowns with selected values
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            // Check for double booking while updating
            var existingBooking = await _context.Bookings
                .Where(b => b.VenueId == booking.VenueId && b.BookingDate == booking.BookingDate && b.BookingId != id)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked on the selected date.");
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
                return View(booking); // Return the view with error messages
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns in case of validation failure
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }

        // Method to prevent deletion of venues or events with active bookings
        public async Task<IActionResult> DeleteVenueOrEvent(int? id, bool isVenue)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (isVenue)
            {
                var venue = await _context.Venues.FindAsync(id);
                if (venue == null)
                {
                    return NotFound();
                }

                // Check if there are active bookings for the venue
                var activeBookings = await _context.Bookings
                    .Where(b => b.VenueId == id)
                    .AnyAsync();

                if (activeBookings)
                {
                    ModelState.AddModelError(string.Empty, "This venue cannot be deleted because there are active bookings.");
                    return RedirectToAction(nameof(Index));
                }

                _context.Venues.Remove(venue);
            }
            else
            {
                var eventToDelete = await _context.Events.FindAsync(id);
                if (eventToDelete == null)
                {
                    return NotFound();
                }

                // Check if there are active bookings for the event
                var activeBookings = await _context.Bookings
                    .Where(b => b.EventId == id)
                    .AnyAsync();

                if (activeBookings)
                {
                    ModelState.AddModelError(string.Empty, "This event cannot be deleted because there are active bookings.");
                    return RedirectToAction(nameof(Index));
                }

                _context.Events.Remove(eventToDelete);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}






/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl");
            return View();
        }


        /*
        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            if (!ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }



        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }

        ////


        // In the Create POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            // Validate for double booking
            var existingBooking = await _context.Bookings
                .Where(b => b.VenueId == booking.VenueId && b.BookingDate == booking.BookingDate)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked on the selected date.");
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // In the Edit POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            // Validate for double booking
            var existingBooking = await _context.Bookings
                .Where(b => b.VenueId == booking.VenueId && b.BookingDate == booking.BookingDate && b.BookingId != id)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked on the selected date.");
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}

*/
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl");
            return View();
        }

        /*
        
        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            if (!ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }
        
        
        
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
{
    // Step 1: Check if the model state is valid
    if (!ModelState.IsValid)
    {
        // Step 2: Check if the venue is available
        var venueAvailable = IsVenueAvailable(booking.VenueId, booking.BookingDate);

        if (!venueAvailable)
        {
            // Add an error to the model state if the venue is already booked
            ModelState.AddModelError("", "The venue is already booked for the selected date.");
        }

        // Step 3: If the model state is still valid after checking availability
        if (ModelState.IsValid)
        {
            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));  // Redirect to Index after successful booking
        }
    }

    // Step 4: If ModelState is invalid, repopulate dropdowns and return the view with the model (errors included)
    ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
    ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
    return View(booking);  // Return view with validation errors
}

// Helper method to check if the venue is available for the selected date
private bool IsVenueAvailable(int? venueId, DateTime bookingDate)
{
    // Look for an existing booking that conflicts with the selected venue and date
    var existingBooking = _context.Bookings
        .Where(b => b.VenueId == venueId && b.BookingDate.Date == bookingDate.Date)
        .FirstOrDefault();

    return existingBooking == null;  // Return true if no conflicting booking is found
}


         




        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,BookingDate,VenueId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Imagegurl", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
*/