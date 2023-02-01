using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class Inspection
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string MachineId { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Serviceman { get; set; }

    public virtual Machine Machine { get; set; } = null!;

    public virtual User ServicemanNavigation { get; set; } = null!;
}
