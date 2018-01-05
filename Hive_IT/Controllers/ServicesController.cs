using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hive_IT.Data;
using Microsoft.AspNetCore.Authorization;
using Hive_IT.Models.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Hive_IT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly CustomerDataContext _db;
        public ServicesController(CustomerDataContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            var model = new ServicesViewModel {
                ManufacturerList = GenerateManufacturerList(),
                ModelList = GenerateModelList(),
                Specific= false
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ServicesViewModel viewModel)
        {
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            viewModel.ManufacturerList = GenerateModelList();
            viewModel.ModelList = GenerateModelList();

            //if specific is false then grab all services where model and manufacturer equal null
            if (viewModel.Specific == false )
            {
                viewModel.Services = _db.Services.Where(serv => serv.Manufacturer == null && serv.Model == null)
                    .OrderBy(serv => serv.Name).ToList();
            }
            else
            {
                // if true then get all services with the associated manufacturer and model
                viewModel.Services = _db.Services.Where(serv => serv.Manufacturer == viewModel.Manufacturer 
                && serv.Model == viewModel.Model).OrderBy(serv => serv.Name).ToList();
            }

            /* Uncomment if we want to fetch via javascript
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Display", viewModel);
            }
            */

            return View("Display" , viewModel);
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            var toAdd = new AddServiceViewModel
            {
                ManufacturerList = GenerateManufacturerList(),
                ModelList = GenerateModelList(),
                NotSpecific = false
                
            };

            return View(toAdd);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddServiceViewModel created)
        {
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            created.ManufacturerList = GenerateManufacturerList();
            created.ModelList = GenerateModelList();

            if (!ModelState.IsValid)
            {
                return View(created);
            }
            

            //if the values of is specific is true set values of model and manufacturer to null, or values are 
            if (created.NotSpecific == true)
            {
                created.Manufacturer = null;
                created.Model = null;
            }

            bool nameExists = false;
            // check if the manufacturer and model fields are empty
            //convert to lowercase so that difference in capitals does not matter
            if (created.Manufacturer == null && created.Model == null)
            {
                nameExists = _db.Services.Any(ser => ser.Name.ToLower() == created.Name.ToLower() && ser.Model == null && ser.Manufacturer == null);
            }
            else
            {
                nameExists = _db.Services.Any(
                    ser => ser.Name.ToLower() == created.Name.ToLower() && ser.Model == created.Model && ser.Manufacturer == created.Manufacturer);
            }
            
            //check if a entry is already in the database with the same name, for specified model and manufacturer
            if (nameExists)
            {
                ModelState.AddModelError("", "That service already exists in the database!");
                return View(created);
            }

            Service converted = new Service
            {
                Name= created.Name,
                Description = created.Description,
                Price = created.Price,
                Manufacturer = created.Manufacturer,
                Model = created.Model
            };

            _db.Services.Add(converted);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
        

        [HttpGet]
        public ActionResult Update(int serviceid)
        {
            var inRecord = _db.Services.FirstOrDefault(serv => serv.ServiceId == serviceid);
            if(inRecord == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var updateModel = new EditServiceViewModel {
                ServiceId = inRecord.ServiceId,
                Name = inRecord.Name,
                Description = inRecord.Description,
                Price = inRecord.Price
            };

            //since services does not have a field to see if specific or not, check values of model and manufacturer
            //if not set for one then it is not specific
            if (inRecord.Manufacturer == null || inRecord.Model == null)
            {
                updateModel.NotSpecific = true;
            }
            else
            {
                updateModel.NotSpecific = false;
                updateModel.Manufacturer = inRecord.Manufacturer;
                updateModel.Model = inRecord.Model;
            }

            return View(updateModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int serviceid, EditServiceViewModel editPending)
        {
            var inRecord = _db.Services.FirstOrDefault(serv => serv.ServiceId == serviceid);
            if (inRecord == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(editPending);
            }

            bool nameExists = false;
            //create a variable to store conflicting service
            var confliction = new Service();
            // check if the manufacturer and model fields are empty
            //convert to lowercase so that difference in capitals does not matter
            if (editPending.Manufacturer == null && editPending.Model == null)
            {
                nameExists = _db.Services.Any(ser => ser.Name.ToLower() == editPending.Name.ToLower() && ser.Model == null && ser.Manufacturer == null);
                confliction = _db.Services.FirstOrDefault(ser => ser.Name.ToLower() == editPending.Name.ToLower() && ser.Model == null && ser.Manufacturer == null);
            }
            else
            {
                nameExists = _db.Services.Any(
                    ser => ser.Name.ToLower() == editPending.Name.ToLower() && ser.Model == editPending.Model && ser.Manufacturer == editPending.Manufacturer);
                confliction = _db.Services.FirstOrDefault(
                    ser => ser.Name.ToLower() == editPending.Name.ToLower() && ser.Model == editPending.Model && ser.Manufacturer == editPending.Manufacturer);
            }

            //check if a entry is already in the database with the same name, for specified model and manufacturer
            if (nameExists)
            {
                //only give error if the service in record that has the same name is not the same as the one being updated
                if (confliction.ServiceId != inRecord.ServiceId)
                {
                    ModelState.AddModelError("", "That service already exists in the database!");
                    return View(editPending);
                }
               
            }

            inRecord.Name = editPending.Name;
            inRecord.Description = editPending.Description;
            inRecord.Price = editPending.Price;

            _db.SaveChanges();

            return RedirectToAction("Index");
        }
        

       [HttpPost]
       [ValidateAntiForgeryToken]
       public ActionResult Delete(int serviceId)
       {
            var toDelete =_db.Services.FirstOrDefault(ser => ser.ServiceId == serviceId);
            if (toDelete == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _db.Services.Remove(toDelete);
            _db.SaveChanges();

           return RedirectToAction(nameof(Index));
       }

        private List<SelectListItem> GenerateManufacturerList()
        {
            var manufacturers = _db.Manufacturers.OrderBy(manu => manu.ManufacturerName).ToList();
            var manufacturerList = new List<SelectListItem>();

            foreach (var manufacturer in manufacturers)
            {
                var listItem = new SelectListItem
                { Value = manufacturer.ManufacturerName, Text = manufacturer.ManufacturerName };
                manufacturerList.Add(listItem);
            }

            return manufacturerList;
        }

        private List<SelectListItem> GenerateModelList(string manufacturer = null)
        {
            var models = new List<ModelofDevice>();
            //if either no manufacturer was passed or manufacturer isnt in db just grab the first that is
            if (manufacturer == null)
            {
                int manufacturerId = _db.Manufacturers.OrderBy(manu => manu.ManufacturerName).FirstOrDefault().ManufacturerId;
                models = _db.DeviceModels.Where(mod => mod.ManufacturerId == manufacturerId).OrderBy(mod => mod.Model).ToList();
            }
            else if (_db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerName == manufacturer) == null)
            {
                int manufacturerId = _db.Manufacturers.OrderBy(manu => manu.ManufacturerName).FirstOrDefault().ManufacturerId;
                models = _db.DeviceModels.Where(mod => mod.ManufacturerId == manufacturerId).OrderBy(mod => mod.Model).ToList();
            }
            else
            {
                int manufacturerId = _db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerName == manufacturer).ManufacturerId;
                models = _db.DeviceModels.Where(mod => mod.ManufacturerId == manufacturerId).OrderBy(mod => mod.ManufacturerId).ToList();
            }

            var modelList = new List<SelectListItem>();

            foreach (var model in models)
            {
                var listItem = new SelectListItem
                { Value = model.Model, Text = model.Model };
                modelList.Add(listItem);
            }

            return modelList;
        }

        //action for ajax call to return a list of manufacturers and first manufacturers model list
        public string ReturnManufacturersAndModels()
        {
            //create the select list items lists
            var ManufacturerList = GenerateManufacturerList();
            var ModelList = GenerateModelList();

            //initialization of list of strings to add to model
            var ManufacturerNames = new List<string>();
            var ModelNames = new List<string>();


            foreach(var manufacturer in ManufacturerList)
            {
                ManufacturerNames.Add(manufacturer.Value);
            }

            foreach (var model in ModelList)
            {
                ModelNames.Add(model.Value);
            }

            var toJson = new ManufacturerAndModelNames {
               Manufacturers = ManufacturerNames,
               Models = ModelNames
            };

            return JsonConvert.SerializeObject(toJson);
        }

    }
}