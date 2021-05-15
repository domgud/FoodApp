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
            //need to do this cause otherwise lazy loading happens and we have a null
            var order = await _context.Order.Include(x => x.Client)
                .Include(x => x.DishOrders)
                .ThenInclude(y => y.Dish)
                .Where(z => z.Id == id).FirstAsync();
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
        


    }
}
