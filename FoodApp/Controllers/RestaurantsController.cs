using FoodApp.Data;
using FoodApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vereyon.Web;
using Microsoft.AspNetCore.Session;
using System.Text.Json;
using FoodApp.Helpers;

namespace FoodApp.Controllers
{
    

    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;
        private readonly UserManager<Restaurant> _userManager;
        private readonly DistanceProcessor _distanceProcessor;


        public RestaurantsController(ApplicationDbContext context, IFlashMessage flashMessage,UserManager<Restaurant> userManager)
        {
            _context = context;
            _flashMessage = flashMessage;
            _userManager = userManager;
            _distanceProcessor = new DistanceProcessor();

        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RequestList()
        {
            return View("RequestList",await _context.Restaurant.Where(r => r.State==Models.Restaurant.RestaurantState.Pending) .ToListAsync());
        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Evaluate(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Confirm(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                    restaurant.State = Models.Restaurant.RestaurantState.Confirmed;
                    _context.Update(restaurant);
                    _flashMessage.Confirmation($"{restaurant.Name} confirmed successfully!");
                    await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(RequestList));
            }
            _flashMessage.Danger($"Something went wrong successfully! :(");
            return RedirectToAction(nameof(RequestList));
        }
        //restaurants requests stuff above
        //guest stuff below
        /////////////////////////////////////////////////////////
        public async Task<IActionResult> Index()
        {
            return View(await _context.Restaurant.Where(x => x.State == Models.Restaurant.RestaurantState.Confirmed).ToListAsync());
        }
        public async Task<IActionResult> Menu(string id)
        {
            if (HttpContext.Session.Get<string>("restaurantId") != id) 
            {
                HttpContext.Session.Set<List<int>>("cart", new List<int>());
                HttpContext.Session.Set<string>("restaurantId", id);
            }
            return View(await _context.Dish.Where(d => d.Restaurant.Id == id).ToListAsync());
        }
        //restaurant registration stuff below
        //////////////////
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return Redirect("/");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register([Bind("Name,Address,Email,Password,ConfirmPassword")] RestaurantRegistrationModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userModel);
            }
            //there are better way to do this, could also use an automapper but oh well :^)
            Restaurant restaurant = new Restaurant();
            restaurant.Name = userModel.Name;
            restaurant.Address = userModel.Address;
            restaurant.Email = userModel.Email;
            restaurant.State = Restaurant.RestaurantState.Pending;
            restaurant.UserName = userModel.Email;
            var result = await _userManager.CreateAsync(restaurant, userModel.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View(userModel);
            }
            await _userManager.AddToRoleAsync(restaurant, "Restaurant");
            _flashMessage.Confirmation($"Registered successfully! You can login now");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        [Authorize(Roles = "Restaurant")]
        public async Task<IActionResult> Orders()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Order
                .Include(x => x.DishOrders)
                .ThenInclude(y => y.Dish)
                .Where(z => z.RestaurantId == id)
                .ToListAsync();
            
            
            return View(orders);
        }
        [Authorize(Roles = "Restaurant")]
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
        [Authorize(Roles = "Restaurant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderDetails(int id, [Bind("Id,DeliveryFee,State,RestaurantId,ClientId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    _flashMessage.Confirmation($"Order state updated successfully!");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Orders));
            }
            return View(order);
        }
        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
        }
        [Authorize(Roles = "User")]
        public async Task <IActionResult> AddToCart(int id)
        {

            List<int> stuff = HttpContext.Session.Get<List<int>>("cart");
            stuff.Add(id);
            HttpContext.Session.Set<List<int>>("cart", stuff);
            _flashMessage.Confirmation("added to cart successfully!", _context.Dish.FindAsync(id).Result.Name);
            return Redirect(Request.Headers["Referer"].ToString());
        }
        private async Task<List<CartViewModel>> CalculatePrice(List<int> dishIds)
        {
            List<CartViewModel> cartItems = new List<CartViewModel>();
            var unique = dishIds.Distinct().ToList();
            var countedDishes = unique.Select((id, count) => new { id, count = dishIds.Count(x => x == id) });
            decimal totalPrice = countedDishes.Sum(item => _context.Dish.Find(item.id).Price * item.count);

            //communicating with the api :^)
            string restaurant = _context.Restaurant.FindAsync(_context.Dish.Find(dishIds.First()).RestaurantId).Result.Address;
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string client = _context.Client.FindAsync(id).Result.Address;
            var distance = await _distanceProcessor.LoadDistance(client, restaurant);
            double distanceceValue = distance.rows.First().elements.First().distance.value;
            double deliveryfee = distanceceValue / 1000 / 4;
            //check if the distance is greater than 25 km
            //then apply additional fee
            if (distanceceValue > 25000)
            {
                deliveryfee = deliveryfee * 1.5;
            }
            //extra payment if price is low
            if (totalPrice < 20)
            {
                deliveryfee += 10;
            }
            foreach (var item in countedDishes)
            {
                Dish dish = await _context.Dish.FindAsync(item.id);
                cartItems.Add(new CartViewModel(dish, dish.Price * item.count, item.count, totalPrice, (decimal)deliveryfee));
            }
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
            order.DeliveryFee =(double) data.DeliveryFee;
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


    }
}
