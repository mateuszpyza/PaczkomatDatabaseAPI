using System.ComponentModel.DataAnnotations;

namespace PaczkomatDatabaseAPI.Models
{
    public class CreateUserDto
    {
        //Podstawowe informacje o użytkowniku
        [Required]
        [Range(100000000,999999999)]
        public int PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Surname { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        public int? AddressId { get; set; }

        // Część adresowa

        [MaxLength(50)]
        public string? Country { get; set; }

        [MaxLength(50)]
        public string? Province { get; set; }

        [MaxLength(50)]
        public string? Town { get; set; }

        [MaxLength(6)]
        public string? PostalCode { get; set; }

        [MaxLength(50)]
        public string? Street { get; set; }

        public short? AddressNumber { get; set; }

    }
}
