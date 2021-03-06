using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [EnumDataType(typeof(OrderState))]
        public OrderState State { get; set; }
        public Client Client { get; set; }
        public Restaurant Restaurant { get; set; }
        public ICollection<DishOrder> DishOrders { get; set; }
        public string ClientId { get; set; }
        public string RestaurantId { get; set; }
        public Order()
        {

        }
    }
}
