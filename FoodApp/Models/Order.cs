using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public enum OrderState
        {
            Taken,
            Paid,
            Confirmed,
            Delivered
        }
        public double DeliveryFee { get; set; }
        public OrderState State { get; set; }
        public Client Client { get; set; }
        public Restaurant Restaurant { get; set; }
        public ICollection<DishOrder> DishOrders { get; set; }
        public Order()
        {

        }
    }
}
