using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string Description { get; set; }

        [ForeignKey("VenueId")]
        public int? VenueId { get; set; }
        public Venue Venue { get; set; }

    }
}
