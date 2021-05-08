﻿using FoodApp.Data;
using FoodApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
            return View(await _context.Restaurant.Where(x => x.State == Models.Restaurant.RestaurantState.Confirmed).ToListAsync());
        }
        public async Task<IActionResult> Menu(string id)
        {
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
    }
}