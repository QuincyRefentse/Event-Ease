using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{

    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        // Correct ForeignKey usage
        [ForeignKey("EventId")]  // Use the foreign key field (EventId) here
        public Event Event { get; set; }

        public int? EventId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [ForeignKey("VenueId")]  // Correctly define ForeignKey for Venue as well
        public Venue Venue { get; set; }
        public int? VenueId { get; set; }
    }

}