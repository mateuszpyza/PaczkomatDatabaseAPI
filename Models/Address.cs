using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class Address
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string Country { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string Town { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string? Street { get; set; }

    public short AddressNumber { get; set; }

    public virtual ICollection<Machine> Machines { get; } = new List<Machine>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
