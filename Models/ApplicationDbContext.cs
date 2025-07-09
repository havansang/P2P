using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace P2P.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Fee> Fees { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionDetail> TransactionDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=MSI\\SQLEXPRESS;Initial Catalog=P2P;user id=sang;password=123456 ; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.FullName).HasMaxLength(250);
            entity.Property(e => e.Password).HasMaxLength(250);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Wallet).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Fee>(entity =>
        {
            entity.ToTable("Fee");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transaction");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankCode).HasMaxLength(50);
            entity.Property(e => e.ConfirmedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DisputedDate).HasColumnType("datetime");
            entity.Property(e => e.DisputedReason).HasMaxLength(250);
            entity.Property(e => e.ExpireDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.Secretkey).HasMaxLength(50);
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(50)
                .HasColumnName("TransactionID");
        });

        modelBuilder.Entity<TransactionDetail>(entity =>
        {
            entity.ToTable("TransactionDetail");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Action)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.PerformedDate).HasColumnType("datetime");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
