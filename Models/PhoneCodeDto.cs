using System.ComponentModel.DataAnnotations;

namespace PaczkomatDatabaseAPI.Models
{
    public class PhoneCodeDto
    {
        [Required]
        [Range(100000000, 999999999)]
        public int PhoneNumber { get; set; }

        [Required]
        [Range(100000, 999999)]
        public int Code { get; set; }

        [Required]
        [Range(100000, 999999)]
        public int MachinePassword { get; set; }
    }
}
