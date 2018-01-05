using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Hive_IT.Data;
using Hive_IT.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hive_IT.Controllers
{
    [Route("roles")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        
        [HttpGet, Route("")]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToArray();
            var roleNames = new List<string>();

            foreach(var role in roles)
            {
                roleNames.Add(role.Name);
            }
            
            var model = new RoleListViewModel { Roles = roleNames};

            return View(model);
        }

        [HttpGet, Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationRole applicationRole)
        {
            var roleName = applicationRole.Name;
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Role must be named");
                return View(applicationRole);
            }

            //just to highlight the importance of this name
            if (roleName.ToLower() == "admin")
            {
                ModelState.AddModelError("", "Role name is reserved. Please pick another.");
            }

            //conversion so that all names follow the capitilize first lower rest convention
            var Capped = System.Globalization.CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(roleName.ToLower());          

            var attemptedRole = await _roleManager.FindByNameAsync(Capped);

            if (attemptedRole == null)
            {
                attemptedRole = new IdentityRole(Capped);
                var result = await _roleManager.CreateAsync(attemptedRole);

                if (!result.Succeeded)
                {
                    foreach(var error in result.Errors.Select(x => x.Description))
                    {
                        ModelState.AddModelError("", error);
                    }

                    return View();
                }

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "That role already exists!");
            return View(applicationRole);
        }
                
        [HttpPost, Route("delete/{roleName}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "No role is identified to delete!");
                return RedirectToAction("Index");
            }

            //prevention of high authority roles deletion
            if (roleName.ToLower() == "admin" || roleName.ToLower() == "manager")
            {
                ModelState.AddModelError("", "That role has deletion disabled");
                return RedirectToAction("Index");
            }

            var queriedRole = await _roleManager.FindByNameAsync(roleName);
            if (queriedRole == null)
            {
                ModelState.AddModelError("", "That role does not exist!");
                return RedirectToAction("Index");

            }

            var Users = await _userManager.GetUsersInRoleAsync(roleName);
            if (Users.Any())
            {
                ModelState.AddModelError("", "Role has users, reassign/delete them first!");
                return RedirectToAction("Index");
            }
            

            var result = await _roleManager.DeleteAsync(queriedRole);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors.Select(e => e.Description))
                {
                    ModelState.AddModelError("", error);
                }

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }
}
