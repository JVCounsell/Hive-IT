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
    //TODO: make it authorized calls
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
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("profile", "account", new {id = User.Identity.Name });
            }

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


            if (login.UserName.ToLower() == "defaultuser")
            {
                var defUser = await _userManager.FindByNameAsync("defaultuser");
                if (!await _userManager.IsInRoleAsync(defUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(defUser, "Admin");
                }
            }

            return RedirectToAction("profile", "account", new { id = login.UserName });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }

        [Authorize]
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
        [Authorize]
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
            
            // check if there are any users assigned with suggested username or email
            var userAssigned = await _userManager.FindByNameAsync(registration.UserName);
            var emailAssigned = await _userManager.FindByEmailAsync(registration.EmailAddress);

            //if so return view with a message
            if (userAssigned != null)
            {
                ModelState.AddModelError("", "Username already taken! Try another.");
                return View(registration);
            }

            if (emailAssigned != null)
            {
                ModelState.AddModelError("", "Email already in use! Are they already another user?");
                return View(registration);
            }

            if (!ModelState.IsValid)
            {
                return View(registration);
            }

            if (registration.UserName.ToLower() == "defaultuser")
            {
                ModelState.AddModelError("", "Specified username is reserved. Please choose another.");
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
         
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List(int page = 0, int sorting=0)
        {
            //page setup group
            var usersPerPage = 15; //TODO: maybe do trial and error or math to calculate what this number should be
            var totalUsers = _userManager.Users.Count();
            var totalPages = totalUsers / usersPerPage;
            var nextPage = page + 1; // probably slightly better to have these calculated just here ...
            var prevPage = page - 1; // instead of multiple places

            //group for view to work with data
            ViewBag.Page = page;
            ViewBag.Prev = prevPage;
            ViewBag.Next = nextPage;
            ViewBag.HasPrevious = prevPage >= 0;
            ViewBag.HasNext = nextPage < totalPages;
            ViewBag.Count = (await _userManager.GetUsersInRoleAsync("Admin")).Count();
            if (0 > sorting || sorting > 2)
            {
                sorting = 0;
            }


            var users = _userManager.Users;
                if (sorting == 0)
            {
                users = users.OrderBy(n => n.UserName);
            } else if (sorting == 1)
            {
                users = users.OrderBy(n => n.FirstName);
            }
            else
            {
                users = users.OrderBy(n => n.LastName);
            }

            //easy to just get sorting as viewbag
            ViewBag.Sorting = sorting;

             var usersSelection = users.Skip(usersPerPage * page).Take(usersPerPage).ToArray();

            var selectedListUsers = new List<ListedUserViewModel>();

            //conversion of application user to new model ensures all the data required is
            //in one eaily accessed model instead of needing role manager as well   
            foreach(var selectedUser in usersSelection)
            {
                var roleIn = await _userManager.GetRolesAsync(selectedUser);

                var newListUser = new ListedUserViewModel
                {
                    First = selectedUser.FirstName,
                    Last = selectedUser.LastName,
                    Username = selectedUser.UserName,
                    Created = selectedUser.DateCreated.ToString("D"),
                    Position = roleIn.First()
                };

                selectedListUsers.Add(newListUser);
            }

            return View(selectedListUsers);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile(string id)
        {
            var userName = id;
            if (string.IsNullOrEmpty(userName))
            {                
                return RedirectToAction("list", "account");
            }

            var currentUser =  await _userManager.FindByNameAsync(userName);

            if(currentUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
                return RedirectToAction("list", "account");
            }

            var rolesUserIsIn = await _userManager.GetRolesAsync(currentUser);

            // made a new model so all the data required is in one model and easier to work with
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

            //need to to know how many admins there are, as cannot allow deletion if only 1
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
             ViewBag.Count= admins.Count();

            return View(userProfile);
        }

        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var userName = id;
            if (string.IsNullOrEmpty(userName))
            {                
                return RedirectToAction("list", "account");
            }

            var currentUser = await _userManager.FindByNameAsync(userName);

            if (currentUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
                return RedirectToAction("list", "account");
            }

            //prevent deletion of user if it is the only admin
            if (await _userManager.IsInRoleAsync(currentUser , "Admin"))
            {
                var admins = (await _userManager.GetUsersInRoleAsync("Admin")).Count();
                if (admins < 2)
                {
                    ModelState.AddModelError("", "Need to always have at least one admin!");
                    return RedirectToAction("list", "account");
                }
            }

            var result = await _userManager.DeleteAsync(currentUser);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors.Select(e => e.Description))
                {
                    ModelState.AddModelError("", error);
                }
                               
                return RedirectToAction("list", "account");
            }

            return RedirectToAction("list", "account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            var userName = id;

            // disable allowance of defaultuser to be edited as should be deleted once new admin created
            if (userName.ToLower() == "defaultuser")
            {
                return RedirectToAction("list", "account");
            }

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
                return RedirectToAction("list", "account");
            }

            var specifiedUser = await _userManager.FindByNameAsync(userName);

            if (specifiedUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
               return RedirectToAction("list", "account");
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

            // setup to prevent changing of admin role if no other user has that role
            if (rolesUserIsIn.First() == "Admin")
            {
                ViewBag.Count = (await _userManager.GetUsersInRoleAsync("Admin")).Count();
            }
            return View(userProfile);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel edit)
        {
            var userName = id;
            ViewBag.Count = (await _userManager.GetUsersInRoleAsync("Admin")).Count();

            // disable allowance of defaultuser to be edited as should be deleted once new admin created
            if (userName.ToLower() == "defaultuser")
            {
                return RedirectToAction("list", "account");
            }

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
                 return RedirectToAction("list", "account");
            }

            var specifiedUser = await _userManager.FindByNameAsync(userName);

            if (specifiedUser == null)
            {
                ModelState.AddModelError("", "Specified user was not found");
                return RedirectToAction("list", "account");
            }
            var rolesUserIsIn = await _userManager.GetRolesAsync(specifiedUser);

            //populate list before sending model back
            edit.RolesList = listOfRoles;

            if (!ModelState.IsValid)
            {
                return View(edit);
            }

            //prevent defaultuser to be taken as username
            if (edit.UserName.ToLower() == "defaultuser")
            {
                ModelState.AddModelError("", "Specified username is reserved. Please choose another.");
                return View(edit);
            }

            //prevent 2 users from using same name
            var nameExists = await _userManager.FindByNameAsync(edit.UserName);
            if (nameExists != null)
            {
                ModelState.AddModelError("", "Username already in use. Choose another.");
                return View(edit);
            }

            //prevent 2 users from using same email
            var emailExists = await _userManager.FindByEmailAsync(edit.EmailAddress);
            if (emailExists != null)
            {
                ModelState.AddModelError("", "Email already in use.");
                return View(edit);
            }

            //reassignment
            specifiedUser.FirstName = edit.FirstName;
            specifiedUser.LastName = edit.LastName;
            specifiedUser.UserName = edit.UserName;
            specifiedUser.Email = edit.EmailAddress;
            specifiedUser.PhoneNumber = edit.PhoneNumber;

            var prevRole = rolesUserIsIn.First();

            //get should prevent this but just for double security so always have an admin
            if (prevRole == "Admin" && ViewBag.Count < 2)
            {
                ModelState.AddModelError("", "Cannot change from admin if there is only one currently!");
                return View(edit);
            }

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
            //cannot get an Application User class just from User.Identity so break into 2 parts
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
