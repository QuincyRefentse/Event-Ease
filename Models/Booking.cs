using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{

    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        // Correct ForeignKey usage
        [Required(ErrorMessage = "Event is required")]
        [ForeignKey("EventId")]  // Use the foreign key field (EventId) here
        public Event Event { get; set; }

        public int? EventId { get; set; }

       // [Required(ErrorMessage = "Bookng date is required")]

        public string ImageBooking { get; set; }

        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [ForeignKey("VenueId")]  // Correctly define ForeignKey for Venue as well
        public Venue Venue { get; set; }
        public int? VenueId { get; set; }
    }

}