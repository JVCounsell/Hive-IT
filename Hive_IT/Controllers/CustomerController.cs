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

        //TODO: Create a List of Customers

        [HttpGet]
        public IActionResult Profile(long id)
        {
            //check to see if customer with specified id exists
            var customerToFind = _db.Customers.FirstOrDefault(x => x.CustomerId == id);
            if (customerToFind == null)
            {
                return RedirectToAction("Create");
            }

            //if it does send all data of customer to new model to be displayed
            var profile = new CustomerProfileViewModel
            {
                Given = customerToFind.FirstName,
                Surname = customerToFind.LastName,
                Creation = customerToFind.DateCreated,
                Phones = _db.PhoneNumbers.Where(p => p.CustomerId == id).ToList(),
                Emails = _db.Emails.Where(e => e.CustomerId == id).ToList(),
                Addresses = _db.MailingAddresses.Where(x => x.CustomerId == id).ToList(),
                CusId = customerToFind.CustomerId
            };

            return View(profile);
        }

        [HttpGet]
        public IActionResult EditNameProfile(long id)
        {
            //check to see if customer with specified id exists
            var customerToFind = _db.Customers.FirstOrDefault(x => x.CustomerId == id);
            if (customerToFind == null)
            {
                return RedirectToAction("Create");
            }

            //if it does send all data of customer to new model to be displayed
            var profile = new CustomerProfileViewModel
            {
                Given = customerToFind.FirstName,
                Surname = customerToFind.LastName,
                Creation = customerToFind.DateCreated,
                Phones = _db.PhoneNumbers.Where(p => p.CustomerId == id).ToList(),
                Emails = _db.Emails.Where(e => e.CustomerId == id).ToList(),
                Addresses = _db.MailingAddresses.Where(x => x.CustomerId == id).ToList(),
                CusId= customerToFind.CustomerId
            };

            return View(profile);
        }

        [HttpPost]
        public IActionResult EditNameProfile(long id, CustomerProfileViewModel edited)
        {
            //check to see if customer with specified id exists
            var customerToFind = _db.Customers.FirstOrDefault(x => x.CustomerId == id);
            if (customerToFind == null)
            {
                return RedirectToAction("Create");
            }

            //assign the rest of the parts of the model with existing data so model can be resent
            edited.Creation = customerToFind.DateCreated;
            edited.CusId = customerToFind.CustomerId;
            edited.Phones = _db.PhoneNumbers.Where(p => p.CustomerId == id).ToList();
            edited.Emails = _db.Emails.Where(e => e.CustomerId == id).ToList();
            edited.Addresses = _db.MailingAddresses.Where(x => x.CustomerId == id).ToList();

            //make sure model is valid
            if (!ModelState.IsValid)
            {
                return View(edited);
            }

            customerToFind.FirstName = edited.Given;
            customerToFind.LastName = edited.Surname;

            _db.SaveChanges();

            return RedirectToAction("profile", "customer", new {id = customerToFind.CustomerId });
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

            // add a new customer to database with current time as creation time
            var addedCustomer = new Customer
            {
                FirstName = added.FirstName,
                LastName = added.LastName,
                DateCreated = DateTime.Now
            };  

            _db.Customers.Add(addedCustomer);
            _db.SaveChanges();

            //save customer to db and find id created so we can work with this data for
            //other tables related to customer
             var newestCustomer = _db.Customers.Last();

            //make sure there is information in email field and if so save to the database
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

            //make sure there is information in phone field and if so save to the database
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

            //create a Customer Address model and fill it
            var addressToAdd = new CustomerAddress
            {
                StreetAddress = added.StreetAddress,
                City = added.City,
                ProvState = added.ProvState,
                Country = added.Country,
                Postal = added.Postal,
                CustomerId = newestCustomer.CustomerId
            };

            var checkedAddress = AnyAddressFieldsFilled(addressToAdd);
                        
            //make sure that one field was filled (if not returned null)
            if (checkedAddress != null)
            {                
                _db.Add(checkedAddress);
                _db.SaveChanges();
            }

            return RedirectToAction("Profile", "customer", new {id = newestCustomer.CustomerId });
        }

        [HttpPost]
        public IActionResult Delete(long id)
        {
            //make sure customer with id exists
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            
            //check if any associated emails exist, and if so remove from db
            if (_db.Emails.Any(e => e.CustomerId == id))
            {
                var linkedEmails = _db.Emails.Where(e => e.CustomerId == id);
                foreach(var linkedEmail in linkedEmails)
                {
                    _db.Remove(linkedEmail);
                }
            }

            //check if any associated phonenumbers exist, and if so remove from db
            if (_db.PhoneNumbers.Any(p => p.CustomerId == id))
            {
                var linkedPhones = _db.PhoneNumbers.Where(p => p.CustomerId == id);
                foreach (var linkedPhone in linkedPhones)
                {
                    _db.Remove(linkedPhone);
                }
            }

            //check if any associated addresses exist, and if so remove from db
            if (_db.MailingAddresses.Any(m => m.CustomerId == id))
            {
                var linkedAddresses = _db.MailingAddresses.Where(m => m.CustomerId == id);
                foreach (var linkedAddress in linkedAddresses)
                {
                    _db.Remove(linkedAddress);
                }
            }

            //remove customer form db and save changes
            _db.Remove(requested);
            _db.SaveChanges();

            //TODO: redirect to list once created
            return RedirectToAction("Create");
        }

        [HttpPost]
        public IActionResult DeleteEmail(long id, string em = null)
        {
            //make sure customer with id exists
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a value for em
            if (string.IsNullOrWhiteSpace(em))
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }

            //make sure there is an attatched email to the customer
            if (_db.Emails.Any(e => e.CustomerId == id))
            {
                //if there is find the one with the associated email id
                var requestedEmail = _db.Emails.FirstOrDefault(e => e.Email == em);
                if (requestedEmail != null)
                {
                    //remove it from db and save
                    _db.Remove(requestedEmail);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Profile", "Customer", new {id = id});
        }

        [HttpPost]
        public IActionResult DeletePhone(long id, long ph = 0)
        {
            //make sure customer with id exists
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a numeric value for ph
            if (ph <= 0)
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }

            //make sure there is an attatched phone to the customer
            if (_db.PhoneNumbers.Any(p => p.CustomerId == id))
            {
                //if there is find the one with the associated phone id
                var requestedPhone = _db.PhoneNumbers.FirstOrDefault(p => p.PhoneId == ph);
                if (requestedPhone != null)
                {
                    //remove it from db and save
                    _db.Remove(requestedPhone);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Profile", "Customer", new { id = id });
        }

        [HttpPost]
        public IActionResult DeleteAddress(long id, long ml = 0)
        {            
            //make sure customer with id exists
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a numeric value for ml
            if (ml <= 0)
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }

            //make sure there is an attatched address to the customer
            if (_db.MailingAddresses.Any(m => m.CustomerId == id))
            {
                //if there is find the one with the associated address id
                var requestedAddress = _db.MailingAddresses.FirstOrDefault(m => m.AddressId == ml);
                if (requestedAddress != null)
                {
                    //remove it from db and save
                    _db.Remove(requestedAddress);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Profile", "Customer", new { id = id });
        }

        [HttpGet]
        public IActionResult EditEmail(long id, string em = null)
        {
            //make sure customer with id exists
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a value for em
            if (string.IsNullOrWhiteSpace(em))
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }
                       
            var editModel = new AddEditEmailViewModel();

            //make sure there is an attatched email to the customer
            if (_db.Emails.Any(e => e.CustomerId == id))
            {
                //if there is find the one with the associated email id
                var requestedEmail = _db.Emails.FirstOrDefault(e => e.Email == em);
                if (requestedEmail == null)
                {
                    return RedirectToAction("Profile", "Customer", new { id = id });
                }
                else
                {
                    editModel = FillEditEmail(requested, editModel);
                    editModel.AlterEmail = requestedEmail;                    
                }
            }

            return View(editModel);
        }

        [HttpPost]
        public IActionResult EditEmail(long id, CustomerEmail editEmail,  string em = null)
        {
            //make sure customer with id exists
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a value for em
            if (string.IsNullOrWhiteSpace(em))
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }

            var edited = new AddEditEmailViewModel
            {
                AlterEmail = editEmail
            };

            //make sure there is an attatched email to the customer
            if (_db.Emails.Any(e => e.CustomerId == id))
            {
                //if there is find the one with the associated email id
                var requestedEmail = _db.Emails.FirstOrDefault(e => e.Email == em);
                if (requestedEmail == null)
                {
                    return RedirectToAction("Profile", "Customer", new { id = id });
                }
                else
                {
                    var sendBack = FillEditEmail(requested, edited);
                    if (!ModelState.IsValid)
                    {
                        return View(sendBack);
                    }

                    if (!string.IsNullOrEmpty(edited.AlterEmail.Email))
                    {
                        requestedEmail.Email = edited.AlterEmail.Email;
                        _db.SaveChanges();
                    }
                }
            }            

            return RedirectToAction("Profile", "Customer", new { id = id });
        }

        [HttpGet]
        public IActionResult EditPhone(long id, long ph)
        {
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a value for ph
            if (ph <= 0)
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }
                        
            var editModel = new AddEditPhoneViewModel();

            //make sure there is an attatched phone to the customer
            if (_db.PhoneNumbers.Any(p => p.CustomerId == id))
            {
                //if there is find the one with the associated phone id
                var requestedPhone = _db.PhoneNumbers.FirstOrDefault(p => p.PhoneId == ph);
                if (requestedPhone == null)
                {
                    return RedirectToAction("Profile", "Customer", new { id = id });
                }
                else
                {
                    editModel = FillEditPhone(requested, editModel);
                    editModel.AlterPhone = requestedPhone;                   
                }
            }

            return View(editModel);
        }

        [HttpPost]
        public IActionResult EditPhone(long id, long ph, CustomerPhoneNumber editPhone)
        {
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a value for ph
            if (ph <= 0)
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }

            var edited = new AddEditPhoneViewModel
            {
                AlterPhone = editPhone
            };

            //make sure there is an attatched phone to the customer
            if (_db.PhoneNumbers.Any(p => p.CustomerId == id))
            {
                //if there is find the one with the associated phone id
                var requestedPhone = _db.PhoneNumbers.FirstOrDefault(p => p.PhoneId == ph);
                if (requestedPhone == null)
                {
                    return RedirectToAction("Profile", "Customer", new { id = id });
                }
                else
                {
                    var sendBack = FillEditPhone(requested, edited);
                    if (!ModelState.IsValid)
                    {
                        return View(sendBack);
                    }

                    if (!string.IsNullOrWhiteSpace(edited.AlterPhone.PhoneNumber))
                    {
                        requestedPhone.PhoneNumber = edited.AlterPhone.PhoneNumber;
                        _db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Profile", "Customer", new { id = id });
        }


        [HttpGet]
        public IActionResult EditAddress(long id, long ml)
        {
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a value for ml
            if (ml <= 0)
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }
                        
            var editModel = new AddEditAddressViewModel();

            //make sure there is an attatched address to the customer
            if (_db.MailingAddresses.Any(m => m.CustomerId == id))
            {
                //if there is find the one with the associated address id
                var requestedAddress = _db.MailingAddresses.FirstOrDefault(m => m.AddressId == ml);
                if (requestedAddress == null)
                {
                    return RedirectToAction("Profile", "Customer", new { id = id });
                }
                else
                {
                    editModel = FillEditAddress(requested, editModel);
                    editModel.AlterAddress = requestedAddress;                   
                }
            }

            return View(editModel);
        }

        [HttpPost]
        public IActionResult EditAddress(long id, CustomerAddress editAdd, long ml)
        {
            var requested = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (requested == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }
            //ensure there is a value for ml
            if (ml <= 0)
            {
                return RedirectToAction("Profile", "Customer", new { id = id });
            }

            var edited = new AddEditAddressViewModel
            {
                AlterAddress = editAdd
            };

            //make sure there is an attatched address to the customer
            if (_db.MailingAddresses.Any(m => m.CustomerId == id))
            {
                //if there is find the one with the associated address id
                var requestedAddress = _db.MailingAddresses.FirstOrDefault(m => m.AddressId == ml);
                if (requestedAddress == null)
                {
                    return RedirectToAction("Profile", "Customer", new { id = id });
                }
                else
                {
                    var sendBack = FillEditAddress(requested, edited);
                    if (!ModelState.IsValid)
                    {
                        return View(sendBack);
                    }

                    var checkedAddress = AnyAddressFieldsFilled(editAdd);
                    if (checkedAddress != null)
                    {
                        requestedAddress.City = checkedAddress.City;
                        requestedAddress.Country = checkedAddress.Country;
                        requestedAddress.Postal = checkedAddress.Postal;
                        requestedAddress.ProvState = checkedAddress.ProvState;
                        requestedAddress.StreetAddress = checkedAddress.StreetAddress;
                        _db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Profile", "Customer", new { id = id });
        }

        [HttpGet]
        public IActionResult AddEmail (long id)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }

            var newEmail = new CustomerEmail { CustomerId = id };
            var adding = new AddEditEmailViewModel { AlterEmail = newEmail };
            adding = FillEditEmail(customer, adding);

            return View(adding);
        }
        
        [HttpPost]
        public IActionResult AddEmail(long id, CustomerEmail addEmail)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }

            var additional = new AddEditEmailViewModel
            {
                AlterEmail = addEmail
            };

            // assign in case it doesnt work
            additional = FillEditEmail(customer, additional);
            
            if (!ModelState.IsValid)
            {
                return View(additional);
            }

            if (!string.IsNullOrWhiteSpace(additional.AlterEmail.Email))
            {
                _db.Add(addEmail);
                _db.SaveChanges();
            }

            return RedirectToAction("profile", "customer", new {id = id });
        }

        [HttpGet]
        public IActionResult AddPhone(long id)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }

            var newPhone = new CustomerPhoneNumber { CustomerId = id};
            var adding = new AddEditPhoneViewModel { AlterPhone = newPhone};
            adding = FillEditPhone(customer, adding);

            return View(adding);
        }

        [HttpPost]
        public IActionResult AddPhone(long id, CustomerPhoneNumber newPhone)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }

            var addedPhone = new AddEditPhoneViewModel
            {
                AlterPhone = newPhone
            };

            addedPhone = FillEditPhone(customer, addedPhone);
            if (!ModelState.IsValid)
            {
                return View(addedPhone);
            }

            if (!string.IsNullOrWhiteSpace(addedPhone.AlterPhone.PhoneNumber))
            {                
                _db.Add(newPhone);
                _db.SaveChanges();
            }

            return RedirectToAction("profile", "customer", new { id = id });
        }

        [HttpGet]
        public IActionResult AddAddress(long id)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }

            var newAdd = new CustomerAddress { CustomerId = id };
            var adding = new AddEditAddressViewModel { AlterAddress = newAdd };
            adding = FillEditAddress(customer, adding);

            return View(adding);
        }

        [HttpPost]
        public IActionResult AddAddress(long id, CustomerAddress newAdd)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                //TODO: redirect to list once created
                return RedirectToAction("Create");
            }

            var added = new AddEditAddressViewModel
            {
                AlterAddress = newAdd
            };

            added = FillEditAddress(customer, added);

            if (!ModelState.IsValid)
            {
                return View(added);
            }

            var checkedAddress = AnyAddressFieldsFilled(added.AlterAddress);
            if (checkedAddress != null)
            {                
                _db.Add(checkedAddress);
                _db.SaveChanges();
            }

            return RedirectToAction("profile", "customer", new { id = id });
        }
            

        //Email View Model filler to fill out customer info
        private AddEditEmailViewModel FillEditEmail (Customer givenCustomer, AddEditEmailViewModel toFill)
        {
            toFill.CusId = givenCustomer.CustomerId;
            toFill.Given = givenCustomer.FirstName;
            toFill.Surname = givenCustomer.LastName;
            toFill.Creation = givenCustomer.DateCreated;
            toFill.Emails = _db.Emails.Where(e => e.CustomerId == givenCustomer.CustomerId).ToList();
            toFill.Phones = _db.PhoneNumbers.Where(p => p.CustomerId == givenCustomer.CustomerId).ToList();
            toFill.Addresses = _db.MailingAddresses.Where(m => m.CustomerId == givenCustomer.CustomerId).ToList();
    
            return toFill;
        }

        //Phone View Model filler to fill out customer info
        private AddEditPhoneViewModel FillEditPhone(Customer givenCustomer, AddEditPhoneViewModel toFill)
        {
            toFill.CusId = givenCustomer.CustomerId;
            toFill.Given = givenCustomer.FirstName;
            toFill.Surname = givenCustomer.LastName;
            toFill.Creation = givenCustomer.DateCreated;
            toFill.Emails = _db.Emails.Where(e => e.CustomerId == givenCustomer.CustomerId).ToList();
            toFill.Phones = _db.PhoneNumbers.Where(p => p.CustomerId == givenCustomer.CustomerId).ToList();
            toFill.Addresses = _db.MailingAddresses.Where(m => m.CustomerId == givenCustomer.CustomerId).ToList();

            return toFill;
        }

        //Address View Model filler to fill out customer info
        private AddEditAddressViewModel FillEditAddress(Customer givenCustomer, AddEditAddressViewModel toFill)
        {
            toFill.CusId = givenCustomer.CustomerId;
            toFill.Given = givenCustomer.FirstName;
            toFill.Surname = givenCustomer.LastName;
            toFill.Creation = givenCustomer.DateCreated;
            toFill.Emails = _db.Emails.Where(e => e.CustomerId == givenCustomer.CustomerId).ToList();
            toFill.Phones = _db.PhoneNumbers.Where(p => p.CustomerId == givenCustomer.CustomerId).ToList();
            toFill.Addresses = _db.MailingAddresses.Where(m => m.CustomerId == givenCustomer.CustomerId).ToList();

            return toFill;
        }

        private CustomerAddress AnyAddressFieldsFilled(CustomerAddress check)
        {            
            var filled = false;

            if (!string.IsNullOrWhiteSpace(check.StreetAddress))
            {                
                filled = true;
            }

            if (!string.IsNullOrWhiteSpace(check.City))
            {
                filled = true;
            }

            if (!string.IsNullOrWhiteSpace(check.ProvState))
            {
                filled = true;
            }

            if (!string.IsNullOrWhiteSpace(check.Country))
            {
                filled = true;
            }

             if (!string.IsNullOrWhiteSpace(check.Postal))
            {
                filled = true;
            }

            if (filled)
            {
                return check;
            }

            return null;
        }
    }
}
