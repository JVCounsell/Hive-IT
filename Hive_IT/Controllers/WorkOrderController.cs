using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hive_IT.Data;

namespace Hive_IT.Controllers
{
    public class WorkOrderController : Controller
    {
        private readonly CustomerDataContext _db;
        //ex. 1:30 PM Friday, Dec 1, 2017
        private readonly string timeFormat = "h:mm tt dddd, MMMM d, yyyy";
        private readonly string numberFormat = "yyMMddHHmmss";

        public WorkOrderController(CustomerDataContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult List()
        {
            return View();
        }
        
        //id is work order number
        [HttpGet]
        public ActionResult Details(string id)
        {
            return View();
        }

        // id is customer id. Should this be changed for more ease of reading? Probably
        [HttpGet]
        public ActionResult Create(long id)
        {
            if (_db.Customers.FirstOrDefault(c => c.CustomerId == id) == null)
            {
                return RedirectToAction("List");
            }
            return View();
        }

        // id is customer id. Should this be changed for more ease of reading? Probably
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(long id, Device created)
        {
            // make sure there is a customer to associate work order to
            if (_db.Customers.FirstOrDefault(c => c.CustomerId == id) == null)
            {
                return RedirectToAction("List");
            }

            if (!ModelState.IsValid)
            {
                return View(created);
            }

            //if the device is fine then generate a new work order
            var workOrder = new WorkOrder
            {
                WorkOrderNumber = DateTime.Now.ToString(numberFormat),
                Status = "Created",
                TimeCreated = DateTime.Now.ToString(timeFormat),
                CustomerId = id
            };

            //save the work order so that there is an entry for the device to associate to
            _db.WorkOrders.Add(workOrder);
            _db.SaveChanges(); 

            created.WorkOrderNumber = workOrder.WorkOrderNumber;

            //will most likely be same as work order since not enough time has passed but this number won't be public
            created.DeviceId = DateTime.Now.ToString(numberFormat);
            created.CreatedAt = workOrder.TimeCreated;

            _db.Devices.Add(created);
            _db.SaveChanges();

            // TODO: redirect to the view(Details?) page once it's completed
            return RedirectToAction(nameof(List));
           
        }

       
        // This should not be an action that happens often at all, will only be admin and should be accessed differently
        //id is work order number
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, WorkOrder order)
        {
            
            // TODO: complete controller

            return RedirectToAction(nameof(List));
            
        }
    }
}