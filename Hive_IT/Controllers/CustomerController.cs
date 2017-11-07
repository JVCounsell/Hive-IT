using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hive_IT.Data;
using Hive_IT.Models.Customers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hive_IT.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerDataContext _db;

        public CustomerController(CustomerDataContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Profile(long id)
        {
            var customerToFind = _db.Customers.FirstOrDefault(x => x.CustomerId == id);
            if (customerToFind == null)
            {
                return RedirectToAction("Create");
            }

            var test = new CustomerProfileViewModel
            {
                Given = customerToFind.FirstName,
                Surname = customerToFind.LastName,
                Creation = customerToFind.DateCreated,
                Phones = _db.PhoneNumbers.Where(p => p.CustomerId == id).ToList(),
                Emails = _db.Emails.Where(e => e.CustomerId == id).ToList(),
                Addresses = _db.MailingAddresses.Where(x => x.CustomerId == id).ToList()
            };

            return View(test);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateCustomerViewModel added)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var addedCustomer = new Customer
            {
                FirstName = added.FirstName,
                LastName = added.LastName,
                DateCreated = DateTime.Now
            };  

            _db.Customers.Add(addedCustomer);
            _db.SaveChanges();

             var newestCustomer = _db.Customers.Last();

            if (!string.IsNullOrWhiteSpace(added.Email))
            {
                var emailToAdd = new CustomerEmail
                {
                    Email = added.Email,
                    CustomerId = newestCustomer.CustomerId
                };

                _db.Add(emailToAdd);
                _db.SaveChanges();
            }
            if (!string.IsNullOrWhiteSpace(added.PhoneNumber))
            {
                var phoneToAdd = new CustomerPhoneNumber
                {
                    PhoneNumber= added.PhoneNumber,
                    CustomerId=newestCustomer.CustomerId
                };
                
                _db.Add(phoneToAdd);
                _db.SaveChanges();                
            }

            var addressToAdd = new CustomerAddress();
            var oneOrMoreFieldFilled = false;

            if (!string.IsNullOrWhiteSpace(added.StreetAddress))
            {
                addressToAdd.StreetAddress = added.StreetAddress;
                oneOrMoreFieldFilled = true;
            }

            if (!string.IsNullOrWhiteSpace(added.City))
            {
                addressToAdd.City = added.City;
                oneOrMoreFieldFilled = true;
            }

            if (!string.IsNullOrWhiteSpace(added.ProvState))
            {
                addressToAdd.ProvState = added.ProvState;
                oneOrMoreFieldFilled = true;
            }

            if (!string.IsNullOrWhiteSpace(added.Country))
            {
                addressToAdd.Country = added.Country;
                oneOrMoreFieldFilled = true;
            }

            if (!string.IsNullOrWhiteSpace(added.Postal))
            {
                addressToAdd.Postal = added.Postal;
                oneOrMoreFieldFilled = true;
            }

            if (oneOrMoreFieldFilled)
            {
                
                addressToAdd.CustomerId = newestCustomer.CustomerId;
                _db.Add(addressToAdd);
                _db.SaveChanges();
            }

            return RedirectToAction("Profile");
        }
        
    }
}
