using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PaczkomatDatabaseAPI.Models;

public partial class PaczkomatDbContext : DbContext
{
    public PaczkomatDbContext()
    {
    }

    public PaczkomatDbContext(DbContextOptions<PaczkomatDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Failure> Failures { get; set; }

    public virtual DbSet<Inspection> Inspections { get; set; }

    public virtual DbSet<Locker> Lockers { get; set; }

    public virtual DbSet<Machine> Machines { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=tcp:paczkomat.database.windows.net,1433;Initial Catalog=Paczkomat;Persist Security Info=False;User ID=CloudSAa20bcf42;Password=P4czk0m4t#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Addresses");

            entity.ToTable("addresses");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressNumber).HasColumnName("address_number");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(50)
                .HasColumnName("postal_code");
            entity.Property(e => e.Province)
                .HasMaxLength(50)
                .HasColumnName("province");
            entity.Property(e => e.Street)
                .HasMaxLength(50)
                .HasColumnName("street");
            entity.Property(e => e.Town)
                .HasMaxLength(50)
                .HasColumnName("town");
        });

        modelBuilder.Entity<Failure>(entity =>
        {
            entity.ToTable("failures");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FixDate)
                .HasColumnType("datetime")
                .HasColumnName("fix_date");
            entity.Property(e => e.Machine)
                .HasMaxLength(10)
                .HasColumnName("machine");
            entity.Property(e => e.OccurDate)
                .HasColumnType("datetime")
                .HasColumnName("occur_date");

            entity.HasOne(d => d.MachineNavigation).WithMany(p => p.Failures)
                .HasForeignKey(d => d.Machine)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_failures_machines");
        });

        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.ToTable("inspections");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.MachineId)
                .HasMaxLength(10)
                .HasColumnName("machine_id");
            entity.Property(e => e.Serviceman).HasColumnName("serviceman");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");

            entity.HasOne(d => d.Machine).WithMany(p => p.Inspections)
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inspections_machines");

            entity.HasOne(d => d.ServicemanNavigation).WithMany(p => p.Inspections)
                .HasForeignKey(d => d.Serviceman)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inspections_users");
        });

        modelBuilder.Entity<Locker>(entity =>
        {
            entity.ToTable("lockers");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.MachineId)
                .HasMaxLength(10)
                .HasColumnName("machine_id");
            entity.Property(e => e.Size)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("size");
            entity.Property(e => e.State)
                .HasMaxLength(10)
                .HasColumnName("state");

            entity.HasOne(d => d.Machine).WithMany(p => p.Lockers)
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_lockers_machines");
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.ToTable("machines");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.Coordinates)
                .HasMaxLength(50)
                .HasColumnName("coordinates");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Address).WithMany(p => p.Machines)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_machines_addresses");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CodeDelivering).HasColumnName("code_delivering");
            entity.Property(e => e.CodeInserting).HasColumnName("code_inserting");
            entity.Property(e => e.CodePicking).HasColumnName("code_picking");
            entity.Property(e => e.CodeReceiving).HasColumnName("code_receiving");
            entity.Property(e => e.DeliveryDate)
                .HasColumnType("datetime")
                .HasColumnName("delivery_date");
            entity.Property(e => e.InsertionDate)
                .HasColumnType("datetime")
                .HasColumnName("insertion_date");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.PickingDate)
                .HasColumnType("datetime")
                .HasColumnName("picking_date");
            entity.Property(e => e.ReceiverLocker)
                .HasMaxLength(10)
                .HasColumnName("receiver_locker");
            entity.Property(e => e.ReceiverMachine)
                .HasMaxLength(10)
                .HasColumnName("receiver_machine");
            entity.Property(e => e.ReceiverUser).HasColumnName("receiver_user");
            entity.Property(e => e.ReceivingDate)
                .HasColumnType("datetime")
                .HasColumnName("receiving_date");
            entity.Property(e => e.SenderLocker)
                .HasMaxLength(10)
                .HasColumnName("sender_locker");
            entity.Property(e => e.SenderMachine)
                .HasMaxLength(10)
                .HasColumnName("sender_machine");
            entity.Property(e => e.SenderUser).HasColumnName("sender_user");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.ReceiverLockerNavigation).WithMany(p => p.OrderReceiverLockerNavigations)
                .HasForeignKey(d => d.ReceiverLocker)
                .HasConstraintName("FK_receiver_locker");

            entity.HasOne(d => d.ReceiverMachineNavigation).WithMany(p => p.OrderReceiverMachineNavigations)
                .HasForeignKey(d => d.ReceiverMachine)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_receiver_machine");

            entity.HasOne(d => d.ReceiverUserNavigation).WithMany(p => p.OrderReceiverUserNavigations)
                .HasForeignKey(d => d.ReceiverUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_receiver_user");

            entity.HasOne(d => d.SenderLockerNavigation).WithMany(p => p.OrderSenderLockerNavigations)
                .HasForeignKey(d => d.SenderLocker)
                .HasConstraintName("FK_sender_locker");

            entity.HasOne(d => d.SenderMachineNavigation).WithMany(p => p.OrderSenderMachineNavigations)
                .HasForeignKey(d => d.SenderMachine)
                .HasConstraintName("FK_sender_machine");

            entity.HasOne(d => d.SenderUserNavigation).WithMany(p => p.OrderSenderUserNavigations)
                .HasForeignKey(d => d.SenderUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_sender_user");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.ToTable("packages");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Size)
                .HasMaxLength(10)
                .HasColumnName("size");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Order).WithMany(p => p.Packages)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_packages_orders");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.PhoneNumber);

            entity.ToTable("users");

            entity.Property(e => e.PhoneNumber)
                .ValueGeneratedNever()
                .HasColumnName("phone_number");
            entity.Property(e => e.AccountType)
                .HasMaxLength(50)
                .HasColumnName("account_type");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");

            entity.HasOne(d => d.Address).WithMany(p => p.Users)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_users_addresses");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
