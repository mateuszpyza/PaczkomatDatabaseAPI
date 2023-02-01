using System;
using System.Collections.Generic;

namespace PaczkomatDatabaseAPI.Models;

public partial class User
{
    public int PhoneNumber { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public int? AddressId { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<Inspection> Inspections { get; } = new List<Inspection>();

    public virtual ICollection<Order> OrderReceiverUserNavigations { get; } = new List<Order>();

    public virtual ICollection<Order> OrderSenderUserNavigations { get; } = new List<Order>();
}
