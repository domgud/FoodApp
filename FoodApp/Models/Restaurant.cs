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
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string State { get; set; }
        public double AverageScore { get; set; }

        public List<Dish> Dishes { get; set; }
        public Restaurant()
        {

        }
    }
}
