using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace PaczkomatDatabaseAPI.Models
{
    public class MachineEventDto
    {
        [Required]
        [MaxLength(10)]
        public string Machine { get; set; } = null!;

        [Required]
        [Range(100000, 999999)]
        public int MachinePassword { get; set; }

        public string? Description { get; set; }
    }
}
