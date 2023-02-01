using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class Package
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string? Description { get; set; }

    public double? Weight { get; set; }

    public string Size { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
