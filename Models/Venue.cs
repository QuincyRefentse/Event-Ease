using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {

        [Key]
        public int VenueId { get; set; }
        [Required]
        public string VenueName { get; set; }
        [Required]
        public string VenueLocaiton { get; set; }
        [Required]
        public string Imagegurl { get; set; }

        [Required]
        public int Capacity { get; set; }
        //public Venue Venue { get; set; }
    }
}
