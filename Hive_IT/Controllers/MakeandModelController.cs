using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hive_IT.Data;
using Hive_IT.Models.MakeandModel;

namespace Hive_IT.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class MakeandModelController : Controller
    {
        private readonly CustomerDataContext _db;
       
        public MakeandModelController(CustomerDataContext db)
        {
            _db = db;            
        }
               
        //Should show all the Manufacturers and Models
        [HttpGet]
        public ActionResult Index()
        {
            var allManufacturers = _db.Manufacturers.OrderBy(manu => manu.ManufacturerName).ToList();
            var allMakesAndModels = new List<ManufacturerModelViewModel>();
            
            foreach(var manufacturer in allManufacturers)
            {
                //create a list of all linked models, ordered ascending by the name as would expect in a list
                var linkedModels = _db.DeviceModels.Where(mod => mod.ManufacturerId == manufacturer.ManufacturerId)
                    .OrderBy(mod => mod.Model).ToList();

                //transform into new model
                var makeandModel = new ManufacturerModelViewModel
                {
                    IDNumber = manufacturer.ManufacturerId,
                    ManufacturerName = manufacturer.ManufacturerName,
                    LinkedModels = linkedModels
                };
                
                allMakesAndModels.Add(makeandModel);
            }

            return View(allMakesAndModels);
        }
                
        // Manufacturer
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // Manufacturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Manufacturer created)
        {
            if (!ModelState.IsValid)
            {
                return View(created);
            }

            //make sure that no repeat names are in the database, ToLower because name doesn't need to be all caps
            var repeatCheck = _db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerName.ToLower() == created.ManufacturerName.ToLower());
            if (repeatCheck != null)
            {
                ModelState.AddModelError("", "There is already a manufacturer with the same name in the Records!");
                return View(created);
            }

            _db.Manufacturers.Add(created);
            _db.SaveChanges();
            
            //after create a manufacture redirect to create some models otherwise why was it added, grab last one as id is incremental
            return RedirectToAction("CreateModel", "MakeandModel", new {manuId = _db.Manufacturers.Last().ManufacturerId });            
        }
        
        [HttpGet]
        public ActionResult CreateModel(int manuId)
        {
            if(_db.Manufacturers.FirstOrDefault(manu =>manu.ManufacturerId == manuId) == null)
            {
                return RedirectToAction("");
            }

            var toAdd = new ModelofDevice { ManufacturerId = manuId };
            return View(toAdd);
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateModel(int manuId, ModelofDevice created)
        {           
            if (_db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerId == manuId) == null)
            {
                return RedirectToAction("");
            }
            //should either add a viewbag or model parameter to pass manufaturername to view

            if (!ModelState.IsValid)
            {
                return View(created);
            }

            //check to see if name is taken only for this manufacturer, use all lower case sovariation on capitals is irrelevant
            var nameExists = _db.DeviceModels.Where(mod => mod.ManufacturerId == manuId)
                .Any(mod => mod.Model.ToLower() == created.Model.ToLower());

            if (nameExists)
            {
                ModelState.AddModelError("", "This Model already exists in the Records!");
                return View(created);
            }

            _db.DeviceModels.Add(created);
            _db.SaveChanges();

            return RedirectToAction("");
        }

        
        [HttpGet]
        public ActionResult Edit(int manuId)
        {
            var toEdit = _db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerId == manuId);
            if ( toEdit == null)
            {
                return RedirectToAction("");
            }
            
            return View(toEdit);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int manuId, Manufacturer edited)
        {
            var unedited = _db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerId == manuId);
            if ( unedited == null)
            {
                return RedirectToAction("");
            }

            if (!ModelState.IsValid)
            {
                return View(edited);
            }

            //check if name exists in db
            var nameExists = _db.Manufacturers.Any(manu => manu.ManufacturerName == edited.ManufacturerName);
            if (nameExists)
            {
                //if name exists check to see whether the name is the same id as the one being edited
                var namedManufaturer = _db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerName == edited.ManufacturerName);
                if (namedManufaturer.ManufacturerId != manuId)
                {
                    ModelState.AddModelError("", "Name already exists within records!");
                    return View(edited);
                }
                else
                {
                    //nothing was changed in this case
                    return RedirectToAction("");
                }
            }

            unedited.ManufacturerName = edited.ManufacturerName;
            _db.SaveChanges();

            return RedirectToAction("");             
        }

        
        [HttpGet]
        public ActionResult EditModel(int modId)
        {
            var toEdit = _db.DeviceModels.FirstOrDefault(mod => mod.Identifier == modId);
            if (toEdit == null)
            {
                return RedirectToAction("");
            }

            return View(toEdit);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditModel(int modId, ModelofDevice edited)
        {
            var unedited = _db.DeviceModels.FirstOrDefault(mod => mod.Identifier == modId);
            if (unedited == null)
            {
                return RedirectToAction("");
            }

            if (!ModelState.IsValid)
            {
                return View(edited);
            }

            //check if name exists in db
            var nameExists = _db.DeviceModels.Any(mod => mod.Model == edited.Model);
            if (nameExists)
            {
                //if name exists check to see whether the name is the same id as the one being edited
                var namedModel = _db.DeviceModels.FirstOrDefault(mod => mod.Model == edited.Model);
                if (namedModel.Identifier != modId)
                {
                    ModelState.AddModelError("", "Name already exists within records!");
                    return View(edited);
                }
                else
                {
                    //nothing was changed in this case
                    return RedirectToAction("");
                }
            }

            unedited.Model = edited.Model;
            _db.SaveChanges();

            return RedirectToAction("");
        }

                
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int manuId)
        {
            var toDelete = _db.Manufacturers.FirstOrDefault(manu => manu.ManufacturerId == manuId);
            if (toDelete == null)
            {
                return RedirectToAction("");
            }

            //see if any devices are connected and if so delete them all from the database
            if (_db.DeviceModels.Any(mod => mod.ManufacturerId == manuId))
            {
                var connectedModels = _db.DeviceModels.Where(mod => mod.ManufacturerId == manuId);
                foreach (var connected in connectedModels)
                {
                    _db.Remove(connected);
                }
            }

            _db.Remove(toDelete);

            return View();            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteModel(int modId)
        {
            //make sure the model exists
            var toDelete = _db.DeviceModels.FirstOrDefault(mod => mod.Identifier == modId);
            if (toDelete == null)
            {
                return RedirectToAction("");
            }
                       
            _db.Remove(toDelete);

            return View();
        }               
    }
}