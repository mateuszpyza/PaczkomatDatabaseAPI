using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class Machine
{
    public string Id { get; set; } = null!;

    public string? Description { get; set; }

    public string Coordinates { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? AddressId { get; set; }

    public string Password { get; set; } = null!;

    public virtual Address? Address { get; set; }

    public virtual ICollection<Failure> Failures { get; } = new List<Failure>();

    public virtual ICollection<Inspection> Inspections { get; } = new List<Inspection>();

    public virtual ICollection<Locker> Lockers { get; } = new List<Locker>();

    public virtual ICollection<Order> OrderReceiverMachineNavigations { get; } = new List<Order>();

    public virtual ICollection<Order> OrderSenderMachineNavigations { get; } = new List<Order>();
}
