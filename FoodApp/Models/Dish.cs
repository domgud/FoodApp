using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApp.Models
{
    public class Dish
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Details { get; set; }
        [Required]
        public decimal Price { get; set; }
        public Restaurant Restaurant { get; set; }
        public string RestaurantId { get; set; }
        public ICollection<DishOrder> DishOrders { get; set; }

        public Dish()
        {

        }
    }
}
