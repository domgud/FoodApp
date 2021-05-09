using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using FoodApp.Models;

namespace FoodApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FoodApp.Models.Dish> Dish { get; set; }
        public DbSet<FoodApp.Models.Restaurant> Restaurant { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<DishOrder> DishOrders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Restaurant>().Property(x => x.State).HasConversion<int>();
            modelBuilder.Entity<Order>().Property(x => x.State).HasConversion<int>();
            modelBuilder.Entity<Dish>().HasOne<Restaurant>(x => x.Restaurant).WithMany(x => x.Dishes).HasForeignKey(x => x.RestaurantId);
            modelBuilder.Entity<Order>().HasOne<Client>(x => x.Client).WithMany(x => x.Orders).HasForeignKey(x => x.ClientId);
            modelBuilder.Entity<Order>().HasOne<Restaurant>(x => x.Restaurant).WithMany(x => x.Orders).HasForeignKey(x => x.RestaurantId);
            modelBuilder.Entity<DishOrder>()
        .HasKey(bc => new { bc.DishId, bc.OrderId });
            modelBuilder.Entity<DishOrder>()
                .HasOne(bc => bc.Dish)
                .WithMany(b => b.DishOrders)
                .HasForeignKey(bc => bc.DishId);
            modelBuilder.Entity<DishOrder>()
                .HasOne(bc => bc.order)
                .WithMany(c => c.DishOrders)
                .HasForeignKey(bc => bc.OrderId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
