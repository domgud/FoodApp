using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApp.Models
{
    public class CartViewModel
    {
        public Dish Dish { get; set; }
        public decimal TotalPrice { get; set; }
        public int ItemCount { get; set; }
        public decimal CartPrice { get; set; }
        public CartViewModel(Dish dish, decimal price, int item, decimal cart)
        {
            Dish = dish;
            TotalPrice = price;
            ItemCount = item;
            CartPrice = cart;
        }
        public CartViewModel()
        {

        }
    }
}
