using Microsoft.EntityFrameworkCore;
using Payments.Shared.Models;

namespace Payments.Application.Data;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<PaymentIntent> PaymentIntents => Set<PaymentIntent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentIntent>(entity =>
        {
            entity.ToTable("payment_intents");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.ConfirmedAt)
                .HasColumnName("confirmed_at");

            entity.Property(e => e.CapturedAt)
                .HasColumnName("captured_at");

            entity.Property(e => e.ReversedAt)
                .HasColumnName("reversed_at");

            entity.Property(e => e.ExpiredAt)
                .HasColumnName("expired_at");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at");

            // Índices para mejorar consultas
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_payment_intents_status");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("ix_payment_intents_created_at");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("ix_payment_intents_expires_at");
        });
    }
}
