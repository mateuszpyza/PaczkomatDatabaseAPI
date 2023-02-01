using System.ComponentModel.DataAnnotations;

namespace PaczkomatDatabaseAPI.Models
{
    public class CreateOrderDto
    {
        [Required]
        [Range(100000000, 999999999)]
        public int SenderUser { get; set; }

        [Required]
        [Range(100000000, 999999999)]
        public int ReceiverUser { get; set; }

        [Required]
        [MaxLength(10)]
        public string? ReceiverMachine { get; set; }

        // Opis paczki

        [Required]
        [MaxLength(7)]
        public string? Size { get; set; }

        [Range(0.0,10.0)]
        public float? Weight { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

    }
}
