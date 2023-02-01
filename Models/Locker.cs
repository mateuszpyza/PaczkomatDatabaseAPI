using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class Locker
{
    public string Id { get; set; } = null!;

    public string MachineId { get; set; } = null!;

    public string? Description { get; set; }

    public string Size { get; set; } = null!;

    public string State { get; set; } = null!;

    public virtual Machine Machine { get; set; } = null!;

    public virtual ICollection<Order> OrderReceiverLockerNavigations { get; } = new List<Order>();

    public virtual ICollection<Order> OrderSenderLockerNavigations { get; } = new List<Order>();
}
