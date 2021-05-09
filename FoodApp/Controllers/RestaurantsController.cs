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

namespace FoodApp.Controllers
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }

    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;
        private readonly UserManager<Restaurant> _userManager;


        public RestaurantsController(ApplicationDbContext context, IFlashMessage flashMessage,UserManager<Restaurant> userManager)
        {
            _context = context;
            _flashMessage = flashMessage;
            _userManager = userManager;

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
            Dictionary<string, int> stuff = new Dictionary<string, int>();
            int[] a = { 1, 2, 3, 1, 1 };
            HttpContext.Session.Set<int[]>("a", a);
            return View(await _context.Restaurant.Where(x => x.State == Models.Restaurant.RestaurantState.Confirmed).ToListAsync());
        }
        public async Task<IActionResult> Menu(string id)
        {
            if (HttpContext.Session.Get<string>("restaurantId") != id) 
            {
                HttpContext.Session.Set<List<int>>("cart", new List<int>());
                HttpContext.Session.Set<string>("restaurantId", id);
            }
            string rId = HttpContext.Session.Get<string>("restaurantId");
            //var a = HttpContext.Session.Get<int[]>("a");
            //var b = a.Distinct().ToList();
            //var anonymours = b.Select((id, count) => new { id, count = a.Count(x => x == id) });
            return View(await _context.Dish.Where(d => d.Restaurant.Id == id).ToListAsync());
        }
        //restaurant registration stuff below
        //////////////////
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }
        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
        }
        public async Task <IActionResult> AddToCart(int id)
        {

            List<int> stuff = HttpContext.Session.Get<List<int>>("cart");
            stuff.Add(id);
            HttpContext.Session.Set<List<int>>("cart", stuff);
            var a = HttpContext.Session.Get<List<int>>("cart");
            //FIXME fix the redirect
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Cart()
        {
            //will need to add some null checks below for extra security
            //cause stuff can die if the cart is empty
            List <CartViewModel> cartItems = new List<CartViewModel>();
            var dishIds = HttpContext.Session.Get<List<int>>("cart");
            var unique = dishIds.Distinct().ToList();
            var countedDishes = unique.Select((id, count) => new { id, count = dishIds.Count(x => x == id) });
            //move this to seperate method when doing distance calculation w google api
            decimal totalPrice = countedDishes.Sum(item => _context.Dish.Find(item.id).Price * item.count);
            foreach (var item in countedDishes)
            {
                Dish dish = await _context.Dish.FindAsync(item.id);
                cartItems.Add(new CartViewModel(dish, dish.Price * item.count, item.count, totalPrice));
            }
            return View(cartItems);
        }
        public async Task<IActionResult> CartCheckout()
        {
            //this is a horrible implementation cause im doing same calculations twice
            // will need to get back to it later :^);
            var dishIds = HttpContext.Session.Get<List<int>>("cart");
            var unique = dishIds.Distinct().ToList();
            var countedDishes = unique.Select((id, count) => new { id, count = dishIds.Count(x => x == id) });
            //move this to seperate method when doing distance calculation w google api
            decimal totalPrice = countedDishes.Sum(item => _context.Dish.Find(item.id).Price * item.count);
            Order order = new Order();
            order.State = Order.OrderState.Paid;
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            order.ClientId = id;
            order.DeliveryFee = (double)totalPrice;
            Dish dish = await _context.Dish.FindAsync(countedDishes.First().id);
            order.RestaurantId = dish.RestaurantId;

            _context.Order.Add(order);
            foreach (var item in countedDishes)
            {
                DishOrder dishorder = new DishOrder();
                dishorder.Dish = _context.Dish.Find(item.id);
                dishorder.DishId = item.id;
                dishorder.order = order;
                dishorder.OrderId = order.Id;
                dishorder.Amount = item.count;
                _context.DishOrders.Add(dishorder);
            }
            await _context.SaveChangesAsync();
            //when implementing everything fully flush the session data before return

            return RedirectToAction(nameof(Index));
        }


    }
}
