using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hive_IT.Models.Account;
using Microsoft.AspNetCore.Identity;
using Hive_IT.Data;
using Hive_IT.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace Hive_IT.Controllers
{
    //TODO: make is authorized calls
    public class AccountController : Controller
    {
        //create places for private readonly managers so Identity data can be worked with
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //assign managers
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            //make sure it is a fresh login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View(new LoginViewModel());
        }

        //need to make async so that we can use methods
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //since remember me isn't wanted, declare rememberance parameter (1st bool) to false
             var result = await _signInManager.PasswordSignInAsync(
                login.UserName, login.Password, false, false
                );

            if (!result.Succeeded) {
                ModelState.AddModelError("", "Login Error!");
                return View();
            }            

            return RedirectToAction("profile", "account", new { id = login.UserName });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            //setup to create a selectable list of all the roles
            var allRoles = _roleManager.Roles.OrderBy(r => r.Name).ToArray();
            var listOfRoles = new List<SelectListItem>();

            //add all to list
            foreach (var role in allRoles)
            {
                var roleItem = new SelectListItem { Value = role.Name, Text = role.Name };
                listOfRoles.Add(roleItem);
            }

            var regModel = new RegisterViewModel { RolesList = listOfRoles };

            return View(regModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registration)
        {
            //setup to create a selectable list of all the roles
            var allRoles = _roleManager.Roles.OrderBy(r => r.Name).ToArray();
            var listOfRoles = new List<SelectListItem>();

            //add all to list
            foreach (var role in allRoles)
            {
                var roleItem = new SelectListItem { Value = role.Name, Text = role.Name };
                listOfRoles.Add(roleItem);
            }

            //assign a selectable list of roles again in case there is some problem
            registration.RolesList = listOfRoles;

            if (!ModelState.IsValid)
            {
                return View(registration);
            }

            //assignment for model data to be converted to database data
            var newUser = new ApplicationUser
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                UserName = registration.UserName,
                Email = registration.EmailAddress,
                PhoneNumber = registration.PhoneNumber,
                DateCreated = DateTime.Now
            };

            var result = await _userManager.CreateAsync(newUser, registration.Password);
            
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors.Select(x => x.Description))
                {
                    ModelState.AddModelError("", error);
                }

                return View();
            }
            
            // finds a role based off of its name
            IdentityRole Role = await _roleManager.FindByNameAsync(registration.Role);

            if (Role == null)
            {
                ModelState.AddModelError("", "Role does not exist!");
                return View(registration);
             }

            IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, Role.Name);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors.Select(x => x.Description))
                {
                    ModelState.AddModelError("", error);
                }

                return View();
            }

            return RedirectToAction("profile", "account", new { id = registration.UserName });
        }
         
        //TODO: Code this
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string id)
        {
            var userName = id;
            if (string.IsNullOrEmpty(userName))
            {
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            var currentUser =  await _userManager.FindByNameAsync(userName);

            if(currentUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            var rolesUserIsIn = await _userManager.GetRolesAsync(currentUser);

            var userProfile = new UserViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                UserName = currentUser.UserName,
                EmailAddress = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber,
                CreationDate = currentUser.DateCreated.ToString("D"),

                //since each user should only have one role just grab the first (only) of list
                Role = rolesUserIsIn.First()
            };
            
            return View(userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var userName = id;
            if (string.IsNullOrEmpty(userName))
            {
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            var currentUser = await _userManager.FindByNameAsync(userName);

            if (currentUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            var result = await _userManager.DeleteAsync(currentUser);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors.Select(e => e.Description))
                {
                    ModelState.AddModelError("", error);
                }

                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            //TODO: Once list users page is created redirect to there
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var userName = id;

            //setup to create a selectable list of all the roles
            var allRoles = _roleManager.Roles.OrderBy(r => r.Name).ToArray();
            var listOfRoles = new List<SelectListItem>();

            //add all to list
            foreach (var role in allRoles)
            {
                var roleItem = new SelectListItem { Value = role.Name, Text = role.Name };
                listOfRoles.Add(roleItem);
            }

            if (string.IsNullOrEmpty(userName))
            {
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            var specifiedUser = await _userManager.FindByNameAsync(userName);

            if (specifiedUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            var rolesUserIsIn = await _userManager.GetRolesAsync(specifiedUser);

            var userProfile = new EditUserViewModel
            {
                FirstName = specifiedUser.FirstName,
                LastName = specifiedUser.LastName,
                UserName = specifiedUser.UserName,
                EmailAddress = specifiedUser.Email,
                PhoneNumber = specifiedUser.PhoneNumber,

                //since each user should only have one role just grab the first (only) of list
                Role = rolesUserIsIn.First(),
                RolesList = listOfRoles
            };

            return View(userProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel edit)
        {
            var userName = id;

            //setup to create a selectable list of all the roles
            var allRoles = _roleManager.Roles.OrderBy(r => r.Name).ToArray();
            var listOfRoles = new List<SelectListItem>();

            //add all to list
            foreach (var role in allRoles)
            {
                var roleItem = new SelectListItem { Value = role.Name, Text = role.Name };
                listOfRoles.Add(roleItem);
            }

            if (string.IsNullOrEmpty(userName))
            {
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }

            var specifiedUser = await _userManager.FindByNameAsync(userName);

            if (specifiedUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
                //TODO: Once list users page is created redirect to there
                return RedirectToAction("index", "home");
            }
            var rolesUserIsIn = await _userManager.GetRolesAsync(specifiedUser);


            //populate list before sending model back
            edit.RolesList = listOfRoles;

            if (!ModelState.IsValid)
            {
                return View(edit);
            }

            //reassignment
            specifiedUser.FirstName = edit.FirstName;
            specifiedUser.LastName = edit.LastName;
            specifiedUser.UserName = edit.UserName;
            specifiedUser.Email = edit.EmailAddress;
            specifiedUser.PhoneNumber = edit.PhoneNumber;

            var prevRole = rolesUserIsIn.First();

            var updateResult = await _userManager.UpdateAsync(specifiedUser);

            if (!updateResult.Succeeded)
            {
                foreach(var err in updateResult.Errors.Select(e => e.Description))
                {
                    ModelState.AddModelError("", err);
                }

                return View();
            }


            // finds a role based off of its name
            IdentityRole Role = await _roleManager.FindByNameAsync(edit.Role);

            if (Role == null)
            {
                ModelState.AddModelError("", "Role does not exist!");
                return View();
            }

            //remove from previous role before addind to more as only one role allowed per user
            var roleDeletion = await _userManager.RemoveFromRoleAsync(specifiedUser, prevRole);
            if (!roleDeletion.Succeeded)
            {                
                foreach (var error in roleDeletion.Errors.Select(x => x.Description))
                {
                    ModelState.AddModelError("", error);
                }

                return View();
            }

            var roleResult = await _userManager.AddToRoleAsync(specifiedUser, Role.Name);
            if (!roleResult.Succeeded)
            {
                //reassign to previous role so that user isn't role-less
                await _userManager.AddToRoleAsync(specifiedUser, prevRole);

                foreach (var error in roleResult.Errors.Select(x => x.Description))
                {
                    ModelState.AddModelError("", error);
                }

                return View();
            }

            return RedirectToAction("profile", "account", new { id = edit.UserName });
            
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            //dont need string id as it only works for current user
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel change)
        {            
            //cannot get an Application User class just from Identity so break into 2 parts
            var currentUsername = User.Identity.Name;
            var currentUser = await _userManager.FindByNameAsync(currentUsername);
            
            if (!ModelState.IsValid)
            {
                return View();
            }

            var changeResult = await _userManager.ChangePasswordAsync(currentUser, change.OldPassword, change.NewPassword);
            if (!changeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failure to change password!");
                return View();
            }

            return RedirectToAction("index", "home");
            //once list is up use this and delete other
            //return RedirectToAction("List", "account");
        }
    }
}
