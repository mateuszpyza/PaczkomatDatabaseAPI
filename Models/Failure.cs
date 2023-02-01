using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class Failure
{
    public int Id { get; set; }

    public string Machine { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime OccurDate { get; set; }

    public DateTime? FixDate { get; set; }

    public virtual Machine MachineNavigation { get; set; } = null!;
}
