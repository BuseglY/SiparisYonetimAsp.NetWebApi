using Microsoft.EntityFrameworkCore;
using SiparisYonetimSistemi.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SiparisYonetimSistemi.Data
{   
        public class OrderDbContext : DbContext
        {
            public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
            {
            }

            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Order configuration
                modelBuilder.Entity<Order>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                    entity.Property(e => e.CreatedAt);

                    entity.HasMany(e => e.OrderItems)
                          .WithOne(e => e.Order)
                          .HasForeignKey(e => e.OrderId)
                          .OnDelete(DeleteBehavior.Cascade);
                });

                // OrderItem configuration
                modelBuilder.Entity<OrderItem>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                    entity.HasOne(e => e.Product)
                          .WithMany(e => e.OrderItems)
                          .HasForeignKey(e => e.ProductId)
                          .OnDelete(DeleteBehavior.Restrict);
                });

                // Product configuration
                modelBuilder.Entity<Product>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Price).HasPrecision(18, 2);
                    entity.Property(e => e.CreatedAt);

                    entity.HasIndex(e => e.Name);
                });

                // Seed data
                modelBuilder.Entity<Product>().HasData(
                    new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 10 },
                    new Product { Id = 2, Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Stock = 50 },
                    new Product { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, Stock = 25 }
                );
            }
        }
    }