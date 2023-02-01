using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class Order
{
    public int Id { get; set; }

    public int SenderUser { get; set; }

    public string? SenderMachine { get; set; }

    public string? SenderLocker { get; set; }

    public int ReceiverUser { get; set; }

    public string ReceiverMachine { get; set; } = null!;

    public string? ReceiverLocker { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? InsertionDate { get; set; }

    public DateTime? PickingDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public DateTime? ReceivingDate { get; set; }

    public string Status { get; set; } = null!;

    public int CodeInserting { get; set; }

    public int? CodePicking { get; set; }

    public int? CodeDelivering { get; set; }

    public int? CodeReceiving { get; set; }

    public virtual ICollection<Package> Packages { get; } = new List<Package>();

    public virtual Locker? ReceiverLockerNavigation { get; set; }

    public virtual Machine ReceiverMachineNavigation { get; set; } = null!;

    public virtual User ReceiverUserNavigation { get; set; } = null!;

    public virtual Locker? SenderLockerNavigation { get; set; }

    public virtual Machine? SenderMachineNavigation { get; set; }

    public virtual User SenderUserNavigation { get; set; } = null!;
}
