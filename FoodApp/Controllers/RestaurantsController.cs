using FoodApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vereyon.Web;


namespace FoodApp.Controllers
{
    
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;

        public RestaurantsController(ApplicationDbContext context, IFlashMessage flashMessage)
        {
            _context = context;
            _flashMessage = flashMessage;
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
        /////////////////////////////////////////////////////////
        public async Task<IActionResult> Index()
        {
            return View(await _context.Restaurant.Where(x => x.State == Models.Restaurant.RestaurantState.Confirmed).ToListAsync());
        }
        public async Task<IActionResult> Menu(string id)
        {
            return View(await _context.Dish.Where(d => d.Restaurant.Id == id).ToListAsync());
        }
    }
}
