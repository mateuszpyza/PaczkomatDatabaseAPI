using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace PaczkomatDatabaseAPI.Models
{
    public class InspectionEventDto
    {
        [Required]
        [Range(100000000,999999999)]
        public int PhoneNumber { get; set; }

        [Required]
        [Range(100000,999999)]
        public int Code { get; set; }

        [Required]
        [MaxLength(10)]
        public string Machine { get; set; } = null!;

        public string? Description { get; set; }
    }
}
