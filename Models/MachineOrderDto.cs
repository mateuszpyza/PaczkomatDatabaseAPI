using System.ComponentModel.DataAnnotations;

namespace PaczkomatDatabaseAPI.Models
{
    public class MachineOrderDto
    {
        [Range(1, 999999999)]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Machine{ get; set; } = string.Empty;

        [Required]
        [Range(100000000, 999999999)]
        public int PhoneNumber { get; set; }

        [Required]
        [Range(100000, 999999)]
        public int Code { get; set; }
    }

}
