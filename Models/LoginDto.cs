using System.ComponentModel.DataAnnotations;

namespace PaczkomatDatabaseAPI.Models
{
    public class LoginDto
    {
        [Required]
        [Range(100000000,999999999)]
        public int PhoneNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;

    }
}
