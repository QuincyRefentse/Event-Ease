using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for the Booking entity
        public DbSet<Booking> Bookings { get; set; }

        // DbSet for the Event entity (assuming it's defined elsewhere)
        public DbSet<Event> Events { get; set; }

        // DbSet for the Venue entity (assuming it's defined elsewhere)
        public DbSet<Venue> Venues { get; set; }

        public DbSet<EventType> EventTypes { get; set; }


    }
}
