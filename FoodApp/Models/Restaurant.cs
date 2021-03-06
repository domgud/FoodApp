using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApp.Models
{
    public class Restaurant:IdentityUser
    {
        public enum RestaurantState
        {
            Confirmed,
            Pending,
            Blocked
        }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public RestaurantState State { get; set; }
        public double AverageScore { get; set; }
        public ICollection<Order> Orders { get; set; }

        public List<Dish> Dishes { get; set; }
        public Restaurant()
        {

        }
    }
}
