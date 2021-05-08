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
    [Authorize(Roles = "Administrator")]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;

        public RequestsController(ApplicationDbContext context, IFlashMessage flashMessage)
        {
            _context = context;
            _flashMessage = flashMessage;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Restaurant.Where(r => r.State==Models.Restaurant.RestaurantState.Pending) .ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
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
                
                return RedirectToAction(nameof(Index));
            }
            _flashMessage.Danger($"Something went wrong successfully! :(");
            return RedirectToAction(nameof(Index));
        }
    }
}
