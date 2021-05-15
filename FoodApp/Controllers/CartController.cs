using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodApp.Helpers;
using FoodApp.Data;
using Vereyon.Web;
using Microsoft.AspNetCore.Identity;
using FoodApp.Models;
using System.Security.Claims;

namespace FoodApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;
        private readonly UserManager<Restaurant> _userManager;
        private readonly DistanceProcessor _distanceProcessor;


        public CartController(ApplicationDbContext context, IFlashMessage flashMessage, UserManager<Restaurant> userManager)
        {
            _context = context;
            _flashMessage = flashMessage;
            _userManager = userManager;
            _distanceProcessor = new DistanceProcessor();

        }
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddToCart(int id)
        {
            List<int> stuff = HttpContext.Session.Get<List<int>>("cart");
            stuff.Add(id);
            HttpContext.Session.Set<List<int>>("cart", stuff);
            var temp = await _context.Dish.FindAsync(id);
            _flashMessage.Confirmation("added to cart successfully!", temp.Name);
            return Redirect(Request.Headers["Referer"].ToString());
        }
        private async Task<List<CartViewModel>> CalculatePrice(List<int> dishIds)
        {
            List<CartViewModel> cartItems = new List<CartViewModel>();
            var unique = dishIds.Distinct().ToList();
            var countedDishes = unique.Select((id, count) => new { id, count = dishIds.Count(x => x == id) });
            decimal totalPrice = countedDishes.Sum(item => _context.Dish.Find(item.id).Price * item.count);

            //communicating with the api :^)
            string restaurant = getRestaurantAddress(dishIds);
            string client = getUserAddress();
            double distanceceValue = await checkDistance(client, restaurant);
            double deliveryfee;

            if (checkIfOrderIsFree())
            {
                deliveryfee = 0;
            }
            else
            {
                deliveryfee = distanceceValue / 1000 / 4;
                //check if the distance is greater than 25 km
                //then apply additional fee
                if (distanceceValue > 25000)
                {
                    deliveryfee = applyHigherTaxRate(deliveryfee);
                }
                else
                {
                    deliveryfee = applyStandartTaxRate(deliveryfee);
                }

                //extra payment if price is low
                if (totalPrice < 20)
                {
                    deliveryfee = applyExtraTax(deliveryfee);
                }
            }

            applyDeliveryPayToDishOrder(dishIds, totalPrice, deliveryfee, cartItems);
            return cartItems;
        }


        [Authorize(Roles = "User")]
        public async Task<IActionResult> Cart()
        {
            var dishIds = HttpContext.Session.Get<List<int>>("cart");
            if (dishIds is null || dishIds.Count < 1)
            {
                _flashMessage.Warning("Please add items to the cart first");
                return Redirect(Request.Headers["Referer"].ToString());
            }

            var cartItems = await CalculatePrice(dishIds);


            return View(cartItems);
        }
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CartCheckout()
        {
            //this is a horrible implementation cause im doing same calculations twice
            // will need to get back to it later :^);

            var dishIds = HttpContext.Session.Get<List<int>>("cart");
            if (dishIds is null || dishIds.Count < 1) return Redirect("/");
            var cartItems = await CalculatePrice(dishIds);
            var data = cartItems.First();
            //creating the new order object
            Order order = new Order();
            order.State = Order.OrderState.Paid;
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            order.ClientId = id;
            order.DeliveryFee = (double)data.DeliveryFee;
            Dish dish = await _context.Dish.FindAsync(data.Dish.Id);
            order.RestaurantId = dish.RestaurantId;
            _context.Order.Add(order);
            foreach (var item in cartItems)
            {
                DishOrder dishorder = new DishOrder();
                dishorder.Dish = _context.Dish.Find(item.Dish.Id);
                dishorder.DishId = item.Dish.Id;
                dishorder.order = order;
                dishorder.OrderId = order.Id;
                dishorder.Amount = item.ItemCount;
                _context.DishOrders.Add(dishorder);
            }
            await _context.SaveChangesAsync();
            _flashMessage.Confirmation("Food is on the way :^)");

            //flushing session cart data
            HttpContext.Session.Set<List<int>>("cart", new List<int>());

            return Redirect("/");
        }

        bool checkIfOrderIsFree()
        {
            //The order is never free dumbo
            return false; 
        }

        string getRestaurantAddress(List<int> dishIds)
        {
            return _context.Restaurant.FindAsync(_context.Dish.Find(dishIds.First()).RestaurantId).Result.Address;
        }

        string getUserAddress()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return  _context.Client.Find(id).Address;
        }

        async Task<double> checkDistance(string client, string restaurant)
        {
            var distance = await _distanceProcessor.LoadDistance(client, restaurant);
            return distance.rows.First().elements.First().distance.value;
        }
        private double applyHigherTaxRate(double deliveryfee)
        {
            return deliveryfee * 1.5;
        }

        private double applyStandartTaxRate(double deliveryfee)
        {
            return deliveryfee;
        }

        private double applyExtraTax(double deliveryfee)
        {
            return deliveryfee + 10;
        }

        private async void applyDeliveryPayToDishOrder(List<int> dishIds, decimal totalPrice, double deliveryfee, List<CartViewModel> cartItems)
        {
            var unique = dishIds.Distinct().ToList();
            var countedDishes = unique.Select((id, count) => new { id, count = dishIds.Count(x => x == id) });
            foreach (var item in countedDishes)
            {
                Dish dish = await _context.Dish.FindAsync(item.id);
                cartItems.Add(new CartViewModel(dish, dish.Price * item.count, item.count, totalPrice, (decimal)deliveryfee));
            }
        }
    }
}
