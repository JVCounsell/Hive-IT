using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hive_IT.Data;
using Hive_IT.Models;
using Hive_IT.Models.WorkOrders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Hive_IT.Controllers
{
    [Authorize]
    public class WorkOrderController : Controller
    {
        private readonly CustomerDataContext _db;
        //ex. 1:30 PM Friday, Dec 1, 2017
        private readonly string timeFormat = "h:mm tt dddd, MMMM d, yyyy";
        private readonly string numberFormat = "yyMMddHHmmss";
        private readonly List<string> workOrderStatuses = new List<string> { "Created", "Paid", "Complete" };
        private readonly List<string> deviceStatuses = new List<string>
        {
            "Created", "Diagnosed", "Being Repaired", "Repaired", "Not Fixable", "Picked Up"
        };

        public WorkOrderController(CustomerDataContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult List(int page = 0, int sorting = 0, int num = 10)
        {
            int ordersPerPage = num;
            int totalOrders = _db.WorkOrders.Count();
            int totalPages = (int)Math.Ceiling(Convert.ToDouble(totalOrders)/ Convert.ToDouble(ordersPerPage));
            int previous = page - 1;
            int next = page + 1;

            ViewBag.Prev = previous;
            ViewBag.Next = next;
            ViewBag.HasPrevious = previous >= 0;
            ViewBag.HasNext = next < totalPages;

            //will need more options for sorting, such as asc or desc each option but ok for now
            //likely pass a string for sorting and a 1 or 0 for asc/desc instead
            if (sorting < 0 || sorting > 2)
            {
                sorting = 0;
            }

            ViewBag.Sorting = sorting;
            ViewBag.Num = num;

            var orders = new List<WorkOrder>();
            if (sorting == 0)
            {
                orders = _db.WorkOrders.OrderByDescending(o => o.WorkOrderNumber).ToList();
            }else if (sorting == 1)
            {
                //sort by desc work order number by status: Complete, Paid, Created
                var completed = _db.WorkOrders.Where(o => o.Status == workOrderStatuses[2])
                    .OrderByDescending(o => o.WorkOrderNumber).ToList();
                var paid = _db.WorkOrders.Where(o => o.Status == workOrderStatuses[1])
                   .OrderByDescending(o => o.WorkOrderNumber).ToList();
                var created = _db.WorkOrders.Where(o => o.Status == workOrderStatuses[0])
                   .OrderByDescending(o => o.WorkOrderNumber).ToList();
                orders = completed.Concat(paid).Concat(created).ToList();
            }
            else
            {
                orders = _db.WorkOrders.OrderByDescending(o => o.StatusLastUpdatedAt).ToList();
            }

            var shownOrders = orders.Skip(page * ordersPerPage).Take(ordersPerPage).ToList();

            var listedOrders = new List<WorkOrderListViewModel>();

            //conversion to model
            foreach (var order in orders)
            {
                var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);
                var listedOrder = new WorkOrderListViewModel
                {
                    OrderNumber = order.WorkOrderNumber,
                    CustomerID = order.CustomerId,
                    CustomerName = customer.FirstName + " " + customer.LastName,
                    OrderStatus = order.Status,
                    DeviceCount = _db.Devices.Count(d => d.WorkOrderNumber == order.WorkOrderNumber), 
                    StatusDate = order.StatusLastUpdatedAt
                };
                               
                listedOrders.Add(listedOrder);
            }

            //use partialview for jquery loading
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(listedOrders);

            return View(listedOrders);
        }
        
        [HttpGet]
        public ActionResult Details(string order)
        {
            // Find associated work order, if it doesn't exist redirect to list of orders 
            var workOrder = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == order);
            if (workOrder == null)
            {
                return RedirectToAction(nameof(List));
            }

            //find associated customer, customer must exist as work order cannot be created without it    
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == workOrder.CustomerId);

            //create a select list item list so work order status can be a select item
            var listOfStatuses = new List<SelectListItem>();
            foreach(var status in workOrderStatuses)
            {
                var listStatus = new SelectListItem { Value = status, Text = status };
                listOfStatuses.Add(listStatus);
            }

            //fill out associated model
            var viewModel = new OrderViewModel
            {
                CustomerID = workOrder.CustomerId,
                CustomerName = customer.FirstName + " " + customer.LastName,
                OrderNumber = workOrder.WorkOrderNumber,
                Status = workOrder.Status,
                StatusChangedAt = workOrder.StatusLastUpdatedAt,
                StatusList = listOfStatuses,
                LinkedDevices = _db.Devices.Where(d => d.WorkOrderNumber == workOrder.WorkOrderNumber).ToList(),
                Emails = _db.Emails.Where(e => e.CustomerId == customer.CustomerId).ToList(),
                Phones = _db.PhoneNumbers.Where(p => p.CustomerId == customer.CustomerId).ToList()
            };
            
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Update(string deviceNumber)
        {
            //find device to update
            var update = _db.Devices.FirstOrDefault(o => o.DeviceId == deviceNumber);
            if (update== null)
            {
                return RedirectToAction(nameof(List));
            }

            var workOrder = _db.WorkOrders.FirstOrDefault(order => order.WorkOrderNumber == update.WorkOrderNumber);
            if(!User.IsInRole("Admin"))
            {
                if (update.Status == "Picked Up" || workOrder.Status == "Completed")
                {
                    return RedirectToAction("Details", "WorkOrder", new { order = update.WorkOrderNumber });
                }                
            }

            //conversion of string to select list item to use in select element
            var listOfStatuses = new List<SelectListItem>();
            foreach(var status in deviceStatuses)
            {
                var statusItem = new SelectListItem {Value= status, Text=status };
                listOfStatuses.Add(statusItem);
            }

            var model = new DeviceViewModel
            {
                DeviceNumber = update.DeviceId,
                Status = update.Status,
                StatusLastUpdatedAt = update.StatusLastUpdatedAt,
                StatusList = listOfStatuses,
                Manufacturer = update.Manufacturer,
                Model = update.Model, 
                Serial = update.Serial,
                Password = update.Password,
                Provider = update.Provider,
                Problem = update.Problem,
                Notes = update.Notes,
                OrderNumber = update.WorkOrderNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(string deviceNumber, DeviceViewModel updatedModel)
        {
            //find device to update
            var update = _db.Devices.FirstOrDefault(o => o.DeviceId == deviceNumber);
            if (update == null)
            {
                return RedirectToAction(nameof(List));
            }

            if (!User.IsInRole("Admin") && update.Status == "Picked Up")
            {
                return RedirectToAction("Details", "WorkOrder", new { order = update.WorkOrderNumber });
            }

            //reinitializing to use in case needs to return model
            var listOfStatuses = new List<SelectListItem>();
            foreach (var status in deviceStatuses)
            {
                var statusItem = new SelectListItem { Value = status, Text = status };
                listOfStatuses.Add(statusItem);
            }
            updatedModel.StatusList = listOfStatuses;

            if (!ModelState.IsValid)
            {
                return View(updatedModel);
            }

            //reassignment block
            update.Manufacturer = updatedModel.Manufacturer;
            update.Model = updatedModel.Model;
            update.Serial = updatedModel.Serial;
            update.Password = updatedModel.Password;
            update.Provider = updatedModel.Provider;
            update.Problem = updatedModel.Problem;
            update.Notes = updatedModel.Notes;

            //initialization for later check
            var initialStatus = update.Status;

            //create a switch to see what the status was changed to and update status and
            //corresponding time field
            switch (updatedModel.Status)
            {               
                case "Diagnosed" :
                    update.Status = "Diagnosed";
                    update.DiagnosedAt = DateTime.Now.ToString(timeFormat);
                    break;
                case "Being Repaired":
                    update.Status = "Being Repaired";
                    update.BegunRepairAt = DateTime.Now.ToString(timeFormat);
                    break;
                case "Repaired":
                    update.Status = "Repaired";
                    update.RepairedAt = DateTime.Now.ToString(timeFormat);
                    break;
                case "Not Fixable":
                    update.Status = "Not Fixable";
                    update.DeclaredUnfixableAt = DateTime.Now.ToString(timeFormat);
                    break;
                case "Picked Up":
                    update.Status = "Picked Up";
                    update.PickedUpAt = DateTime.Now.ToString(timeFormat);
                    break;
                default:
                    update.Status = "Created";
                    break;
            }

            //check to see if the status is updated and if so change corresponding field
            if (initialStatus != update.Status)
            {
                update.StatusLastUpdatedAt = DateTime.Now.ToString(timeFormat);
            }

            _db.SaveChanges();

            return RedirectToAction("Details", "WorkOrder", new {order = update.WorkOrderNumber });
        }

        // id is customer id.
        [HttpGet]
        public ActionResult Create(long id)
        {
            // Find associated customer, if it doesn't exist redirect to list of customers
            if (_db.Customers.FirstOrDefault(c => c.CustomerId == id) == null)
            {
                return RedirectToAction("List", "Customer");
            }
            return View();
        }

        // id is customer id.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(long id, Device created)
        {
            // make sure there is a customer to associate work order to
            if (_db.Customers.FirstOrDefault(c => c.CustomerId == id) == null)
            {
                return RedirectToAction("List", "Customer");
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
                CustomerId = id,
                StatusLastUpdatedAt = DateTime.Now.ToString(timeFormat)
            };

            //save the work order so that there is an entry for the device to associate to
            _db.WorkOrders.Add(workOrder);
            _db.SaveChanges(); 

            created.WorkOrderNumber = workOrder.WorkOrderNumber;

            //will most likely be same as work order since not enough time has passed but this number won't be public
            created.DeviceId = DateTime.Now.ToString(numberFormat);
            created.CreatedAt = workOrder.TimeCreated;
            created.StatusLastUpdatedAt = workOrder.TimeCreated;

            _db.Devices.Add(created);
            _db.SaveChanges();
            
            return RedirectToAction("Details", "WorkOrder", new { order = created.WorkOrderNumber });
           
        }

        [HttpGet]
        public ActionResult Add(string order)
        {
            //make sure there is a workorder to connect to
            if (_db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == order) == null)
            {
                return RedirectToAction(nameof(List));
            }

            var newDevice = new Device { WorkOrderNumber = order };

            return View(newDevice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(string order, Device created)
        {
            //make sure there is a workorder to connect to
            if (_db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == order) == null)
            {
                return RedirectToAction(nameof(List));
            }

            if (!ModelState.IsValid)
            {
                return View(created);
            }
            
            //give id and fill status times
            created.DeviceId = DateTime.Now.ToString(numberFormat);
            created.CreatedAt = DateTime.Now.ToString(timeFormat);
            created.StatusLastUpdatedAt = DateTime.Now.ToString(timeFormat);

            _db.Devices.Add(created);
            _db.SaveChanges();

            return RedirectToAction("Details", "WorkOrder", new { order = order });

        }

        // This should not be an action that happens often at all, will only be admin and should be accessed differently
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string orderNumber)
        {
            //find work order to delete
            var toDelete = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == orderNumber);
            if (toDelete == null)
            {
                return RedirectToAction(nameof(List));
            }

            if (_db.Devices.Any(d => d.WorkOrderNumber == toDelete.WorkOrderNumber))
            {
                var linkedDevices = _db.Devices.Where(d => d.WorkOrderNumber == toDelete.WorkOrderNumber);
                foreach(var device in linkedDevices)
                {
                    _db.Remove(device);
                }
            }

            _db.Remove(toDelete);
            _db.SaveChanges();

            return RedirectToAction(nameof(List));
            
        }

        //only admins for this?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDevice(string deviceNumber)
        {
            //find device to delete
            var toDelete = _db.Devices.FirstOrDefault(o => o.DeviceId == deviceNumber);
            if (toDelete == null)
            {
                return RedirectToAction(nameof(List));
            }            

            _db.Remove(toDelete);
            _db.SaveChanges();

            return RedirectToAction("Details", "WorkOrder", new {order = toDelete.WorkOrderNumber });

        }
    }
}