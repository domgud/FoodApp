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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Restaurant>().Property(x => x.State).HasConversion<int>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
