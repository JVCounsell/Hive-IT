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
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace Hive_IT.Controllers
{
    [Authorize]
    public class WorkOrderController : Controller
    {
        private readonly CustomerDataContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        //ex. 1:30 PM Friday, Dec 1, 2017
        private readonly string timeFormat = "h:mm tt dddd, MMMM d, yyyy";
        private readonly string numberFormat = "yyMMddHHmmss";
        private readonly List<string> workOrderStatuses = new List<string> { "Created", "Paid", "Complete" };
        private readonly List<string> deviceStatuses = new List<string>
        {
            "Created", "Diagnosed", "Being Repaired", "Repaired", "Not Fixable", "Picked Up"
        };
       
        public WorkOrderController(CustomerDataContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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
                Phones = _db.PhoneNumbers.Where(p => p.CustomerId == customer.CustomerId).ToList(),
                ShippingAddresses = _db.MailingAddresses.Where(address => address.CustomerId == customer.CustomerId).ToList()
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(viewModel);
            }

            return View(viewModel);
        }

        //will be an ajax call to get this, return bool for whether action is successfully changing or not
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> Details(string order, string Status)
        {
            // Find associated work order, if it doesn't exist redirect to list of orders 
            var workOrder = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == order);
            if (workOrder == null)
            {
                return false;
            }

            //find associated customer, customer must exist as work order cannot be created without it    
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == workOrder.CustomerId);

            //initialization for later check
            var initialStatus = workOrder.Status;

            //create a switch to see what the status was changed to and update status and
            //corresponding time field
            switch (Status)
            {
                case "Paid":
                    workOrder.Status = "Paid";
                    workOrder.PaidAt = DateTime.Now.ToString(timeFormat);
                    break;
                case "Complete":
                    workOrder.Status = "Complete";
                    workOrder.CompletionAt = DateTime.Now.ToString(timeFormat);
                    break;                
                default:
                    workOrder.Status = "Created";
                    break;
            }

            //check to see if the status is updated and if so change corresponding field and update history
            if (initialStatus != workOrder.Status)
            {
                workOrder.StatusLastUpdatedAt = DateTime.Now.ToString(timeFormat);

                // grab the Application User by the current user's username so UserId can be grabbed
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var History = new WorkOrderHistory
                {
                    ActionTaken = "Updated Status",
                    FieldValueBefore = initialStatus,
                    FieldValueAfter = workOrder.Status,
                    WorkOrderNumber= workOrder.WorkOrderNumber,
                    Username = User.Identity.Name,
                    UserId = user.Id,
                    TimeOfAction = DateTime.Now.ToString(timeFormat),
                    HistoryID = DateTime.Now.ToString(numberFormat + "fff")
                };

                _db.Histories.Add(History);
            }           

            _db.SaveChanges();
            return true;
        }

        [HttpGet]
        public async Task<IActionResult> History(string order)
        {
            // Find associated work order, if it doesn't exist redirect to list of orders             
            if (_db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == order) == null)
            {
                return RedirectToAction(nameof(List));
            }

            ViewBag.OrderNumber = order;

            //find all histories linked to the work order
            var histories = _db.Histories.Where(his => his.WorkOrderNumber == order);

            /*
             * ******************* Done to limit frequent Trips to database ******************************
             */
            //all unique user ids in the associated histories
            var allUniqueUserIds = new List<string>();
            if (histories.Any())
            {
                foreach (var history in histories)
                {
                    //if there is no entries in count add first histories user id to list
                    if (allUniqueUserIds.Count == 0)
                    {
                        allUniqueUserIds.Add(history.UserId);
                    }
                    else
                    {
                        //otherwise see if the histories id exists in list
                        if (!allUniqueUserIds.Exists(id => id == history.UserId))
                        {
                            //if not add it
                            allUniqueUserIds.Add(history.UserId);
                        }
                    }
                }
            }

            //creation of a dictionary that will display the username of the Id
            var userDictionary = new Dictionary<string, string>();

            //but will check if allUniqueUserIds has at least one value otherwise it is pointless
            if(allUniqueUserIds.Count > 0)
            {
                foreach (var userID in allUniqueUserIds)
                {
                    // check to see if there is a user in the database with the saved id (won't be if they are deleted)
                    var selectedUser = await _userManager.FindByIdAsync(userID);

                    if (selectedUser != null)
                    {
                        //if user is still in database assign the username to the userID in the dictionary
                       userDictionary.Add(userID , selectedUser.UserName);
                    }
                    else
                    {
                        //assign a Notification string that specifies the user is no longer in the database
                        userDictionary.Add(userID, "Removed");
                    }
                }
            }

            var allUniqueDeviceIds = new List<string>();
            if (histories.Any())
            {
                foreach (var history in histories)
                {
                    //if there is no entries in count add first histories device id to list
                    if (allUniqueDeviceIds.Count == 0)
                    {
                        //but only if it has a device id
                        if(history.DeviceID != null)
                        {
                            allUniqueDeviceIds.Add(history.DeviceID);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        //but only if it has a device id
                        if (history.DeviceID != null)
                        {
                            if (!allUniqueDeviceIds.Exists(id => id == history.DeviceID))
                            {
                                //if not add it
                                allUniqueDeviceIds.Add(history.DeviceID);
                            }
                            else
                            {
                                continue;
                            }
                        }                        
                        else
                        {
                            continue;
                        }                       
                    }
                }
            }

            //creation of a dictionary that will display the identifier of the device (Details action style format)
            var deviceDictionary = new Dictionary<string, string>();

            //but will check if allUniqueDeviceIds has at least one value otherwise it is pointless
            if (allUniqueDeviceIds.Count > 0)
            {
                foreach (var devID in allUniqueDeviceIds)
                {
                    //check to see if device still exists
                    var selectedDevice = _db.Devices.FirstOrDefault(dev => dev.DeviceId == devID);

                    if (selectedDevice != null)
                    {
                        //if device is still in database assign the username to the userID in the dictionary
                        deviceDictionary.Add(devID, selectedDevice.Manufacturer + " " + selectedDevice.Model + " SN: " + selectedDevice.Serial);
                    }
                    else
                    {
                        //assign a Notification string that specifies the Device is no longer in the database
                        deviceDictionary.Add(devID, "Removed");
                    }
                }
            }
            /*
             * *************************************************************************
             */

            //creation of a empty set of the view models to be shown
            var historiesViewModel = new List<HistoryViewModel>();
            //make sure there are entries in the database (always should be)
            if (histories.Any())
            {
                foreach (var history in histories)
                {
                    //assignment to viewmodel for ease of display
                    var historyModel = new HistoryViewModel
                    {
                        HistId = history.HistoryID,
                        WorkOrderNumber = history.WorkOrderNumber,
                        ActionTaken = history.ActionTaken,
                        TimeOfAction = history.TimeOfAction,
                        FieldValueBefore = history.FieldValueBefore,
                        FieldValueAfter = history.FieldValueAfter,
                        DeviceID = history.DeviceID
                    };

                    string userIdentity = userDictionary[history.UserId];

                    if (userIdentity != "Removed")
                    {
                        //if user is still in database assign the variable to the username currently being used
                        historyModel.Username = userIdentity;
                    }
                    else
                    {
                        //assign to the saved username so people have some semblance of who performed the task
                        historyModel.Username = history.Username;
                    }

                    //in case device id exists
                    if (history.DeviceID != null)
                    {
                        string deviceName = deviceDictionary[history.DeviceID];
                        if (deviceName != "Removed")
                        {
                            //create a identity based on current manufacturer, model and device like in Details
                            historyModel.DeviceSerialIdentifier = deviceName;
                            historyModel.IsDeviceActive = true;
                        }
                        else
                        {
                            //set the identity to what was saved in database as a understandable identification
                            historyModel.DeviceSerialIdentifier = history.DeviceIdentity;
                            historyModel.IsDeviceActive = false;
                        }
                    }

                    historiesViewModel.Add(historyModel);
                }
            }
            //sort latest first
            var Ordered = historiesViewModel.OrderByDescending(his => his.HistId).ToList();
               
            return View(Ordered);
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

            //if there are no manufaturers in db, return to a view that says this, can make work orders without them
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
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
                Manufacturers = GenerateManufacturerList(),
                Model = update.Model, 
                Models = GenerateModelList(update.Manufacturer),
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
        public async Task<IActionResult> Update(string deviceNumber, DeviceViewModel updatedModel)
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

            //if there are no manufaturers in db, return to a view that says this, can make work orders without them,
            //should not happen in a post but just for security
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            //reinitializing to use in case needs to return model
            var listOfStatuses = new List<SelectListItem>();
            foreach (var status in deviceStatuses)
            {
                var statusItem = new SelectListItem { Value = status, Text = status };
                listOfStatuses.Add(statusItem);
            }
            updatedModel.StatusList = listOfStatuses;
           
            updatedModel.Manufacturers = GenerateManufacturerList();
            updatedModel.Models = GenerateModelList(update.Manufacturer);

            if (!ModelState.IsValid)
            {
                return View(updatedModel);
            }

            //reassignment block, check whether each field is updated, and each that is create a history for
            if (update.Manufacturer != updatedModel.Manufacturer)
            {
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Manufacturer";
                history.FieldValueBefore = update.Manufacturer;
                history.FieldValueAfter = updatedModel.Manufacturer;

                _db.Histories.Add(history);
            }
            update.Manufacturer = updatedModel.Manufacturer;
            if (update.Model != updatedModel.Model)
            {
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Model";
                history.FieldValueBefore = update.Model;
                history.FieldValueAfter = updatedModel.Model;

                _db.Histories.Add(history);
            }
            update.Model = updatedModel.Model;
            if (update.Serial != updatedModel.Serial)
            {
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Serial";
                history.FieldValueBefore = update.Serial;
                history.FieldValueAfter = updatedModel.Serial;

                _db.Histories.Add(history);
            }
            update.Serial = updatedModel.Serial;
            if (update.Password != updatedModel.Password)
            {
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Password";
                history.FieldValueBefore = update.Password;
                history.FieldValueAfter = updatedModel.Password;

                _db.Histories.Add(history);
            }
            update.Password = updatedModel.Password;
            if (update.Provider != updatedModel.Provider)
            {
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Provider";
                history.FieldValueBefore = update.Provider;
                history.FieldValueAfter = updatedModel.Provider;

                _db.Histories.Add(history);
            }
            update.Provider = updatedModel.Provider;
            if (update.Problem != updatedModel.Problem)
            {
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Problem";
                history.FieldValueBefore = update.Problem;
                history.FieldValueAfter = updatedModel.Problem;

                _db.Histories.Add(history);
            }
            update.Problem = updatedModel.Problem;
            if (update.Notes != updatedModel.Notes)
            {
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Notes";
                history.FieldValueBefore = update.Notes;
                history.FieldValueAfter = updatedModel.Notes;

                _db.Histories.Add(history);
            }
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

            //check to see if the status is updated and if so change corresponding field and create history
            if (initialStatus != update.Status)
            {
                update.StatusLastUpdatedAt = DateTime.Now.ToString(timeFormat);
                var history = await GenerateBaseHistoryOnUpdation(update);
                history.ActionTaken = "Updated Device Status";
                history.FieldValueBefore = initialStatus;
                history.FieldValueAfter = update.Status;

                _db.Histories.Add(history);
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

            //if there are no manufaturers in db, return to a view that says this, can make work orders without them
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            var toAdd = new AddDeviceViewModel {
                Manufacturers = GenerateManufacturerList(),
                Models = GenerateModelList()
            };

            return View(toAdd);
        }

        // id is customer id.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long id, AddDeviceViewModel created)
        {
            // make sure there is a customer to associate work order to
            if (_db.Customers.FirstOrDefault(c => c.CustomerId == id) == null)
            {
                return RedirectToAction("List", "Customer");
            }

            //if there are no manufaturers in db, return to a view that says this, can make work orders without them,
            //should not happen in a post but just for security
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            //reassignment in case model is invalid
            created.Manufacturers = GenerateManufacturerList();
            created.Models = GenerateModelList();

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

            var createdDevice = GenerateDeviceFromViewModel(created, workOrder.WorkOrderNumber);
            
            _db.Devices.Add(createdDevice);
            _db.SaveChanges();

            //creation of History with the generic function and filling in the applicable details after
            var historyEntry = await GenerateBaseHistoryForDeviceWithoutUpdates(createdDevice, createdDevice.DeviceId);
            historyEntry.ActionTaken = "Created Order and Device";
            historyEntry.TimeOfAction = DateTime.Now.ToString(timeFormat);
            historyEntry.HistoryID = DateTime.Now.ToString(numberFormat + "fff");

            _db.Histories.Add(historyEntry);
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
            //if there are no manufaturers in db, return to a view that says this, can make work orders without them
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            var newDevice = new AddDeviceViewModel {
                WorkOrderNumber = order,
                Manufacturers = GenerateManufacturerList(),
                Models = GenerateModelList()
            };

            return View(newDevice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string order, AddDeviceViewModel created)
        {
            //make sure there is a workorder to connect to
            var workOrder = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == order);
            if ( workOrder == null)
            {
                return RedirectToAction(nameof(List));
            }
            //if there are no manufaturers in db, return to a view that says this, can make work orders without them,
            //should not happen in a post but just for security
            if (_db.Manufacturers.Any() == false)
            {
                return View("NoManufacturers");
            }
            //if there are no models in db, code still functions on page, but better to give the warning
            if (_db.DeviceModels.Any() == false)
            {
                return View("NoManufacturers");
            }

            created.Manufacturers = GenerateManufacturerList();
            created.Models = GenerateModelList();

            if (!ModelState.IsValid)
            {
                return View(created);
            }

            var createdDevice = GenerateDeviceFromViewModel(created, workOrder.WorkOrderNumber);
                       
            _db.Devices.Add(createdDevice);
            _db.SaveChanges();

            //creation of History with the generic function and filling in the applicable details after
            var historyEntry = await GenerateBaseHistoryForDeviceWithoutUpdates(createdDevice, createdDevice.DeviceId);
            historyEntry.ActionTaken = "Created Device";
            historyEntry.TimeOfAction = DateTime.Now.ToString(timeFormat);
            historyEntry.HistoryID = DateTime.Now.ToString(numberFormat + "fff");

            _db.Histories.Add(historyEntry);
            _db.SaveChanges();

            return RedirectToAction("Details", "WorkOrder", new { order });

        }

        [HttpGet]
        public ActionResult Invoice(string orderNumber)
        {
            var workOrder = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == orderNumber);
            //make sure there is a workorder to connect to
            if (workOrder == null)
            {
                return RedirectToAction(nameof(List));
            }
            //if there are no services in db, return to a view that says this
            if (_db.Services.Any() == false)
            {
                return View("NoServices");
            }

            //find associated customer, customer must exist as work order cannot be created without it    
            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == workOrder.CustomerId);

            //create a select list item list so work order status can be a select item
            var listOfStatuses = new List<SelectListItem>();
            foreach (var status in workOrderStatuses)
            {
                var listStatus = new SelectListItem { Value = status, Text = status };
                listOfStatuses.Add(listStatus);
            }

            var invoiceModel = new InvoiceViewModel
            {
                OrderNumber = orderNumber,
                Status = workOrder.Status,
                StatusList = listOfStatuses,
                CustomerName = customer.FirstName + " " + customer.LastName,
                Phone = _db.PhoneNumbers.FirstOrDefault(ph => ph.CustomerId == customer.CustomerId),
                Email = _db.Emails.FirstOrDefault(em => em.CustomerId == customer.CustomerId),
                ShippingAddress = _db.MailingAddresses.FirstOrDefault(ml => ml.CustomerId == customer.CustomerId),
                LinkedDevices = _db.Devices.Where(dev => dev.WorkOrderNumber == orderNumber).ToList()
            };

            //assignment for tally
            float due = 0;
            //assignment to be used
            var doneServices = new List<Service>();
            var serviceCount = new Dictionary<int, int>();

            //get all entities of Service that are linked
            var services = _db.WorkOrders.Where(ord => ord.WorkOrderNumber == orderNumber).SelectMany(ord =>ord.WorkOrderServices).Select(x => x.Service);           
            //then only if there are any
            if(services != null)
            {
                //go through each
                foreach (var ser in services)
                {
                    //find and save the total number of that service linked
                    int num = _db.WorkOrders.Where(ord => ord.WorkOrderNumber == orderNumber)
                        .SelectMany(ord => ord.WorkOrderServices).FirstOrDefault(wos => wos.WorkOrderNumber == orderNumber
                        && wos.ServiceId == ser.ServiceId).NumberOf;

                    //save service id and number to dictionary
                    serviceCount.Add(ser.ServiceId, num);

                    //add the service associated to the list, then add its price * number of them
                    doneServices.Add(ser);
                    due += (ser.Price * num);
                }
            }

            invoiceModel.ServicesDone = doneServices;
            invoiceModel.AmountDue = due;
            invoiceModel.NumberOfServices = serviceCount;

            //assignment of list of services that apply
            var applicableServices = new List<SelectListItem>();
            var messageItem = new SelectListItem {Text = "Select a Service", Value = "Select a Service" };
            applicableServices.Add(messageItem);

            //assignment of new list of devices
            var noRepeatingDeviceList = new List<Device>();

            //order by ascending first manufacturer then model so list follows suit
            var orderedDevices = invoiceModel.LinkedDevices.OrderBy(dev => dev.Manufacturer).ThenBy(dev => dev.Model);
            foreach(var device in orderedDevices)
            {
                if (!noRepeatingDeviceList.Any(dev => dev.Manufacturer == device.Manufacturer && dev.Model == device.Model))
                {
                    noRepeatingDeviceList.Add(device);
                }
            }

            foreach (var device in noRepeatingDeviceList)
            {
                //get all services linked with the same manufacturer and model
                var deviceServices = _db.Services.Where(serv => serv.Manufacturer == device.Manufacturer
                && serv.Model == device.Model).OrderBy(serv =>serv.Name).ToList();

                foreach (var deviceService in deviceServices)
                {
                    /*each service gets converted into a select list item so it can be selected, with text as the manufacturer
                     then model then name as there could be a comparable name for another, and value is a string version of
                    the service id as value does not along ints, service id is value as its easier to lookup*/
                    var selectService = new SelectListItem
                    {
                        Text = deviceService.Manufacturer + " " + deviceService.Model + " " + deviceService.Name,
                        Value = deviceService.ServiceId.ToString()
                    };

                    applicableServices.Add(selectService);
                }
            }

            //do similar for services that don't have a model or manufacturer
            var genericServices = _db.Services.Where(serv => serv.Manufacturer == null && serv.Model == null).OrderBy(serv =>serv.Name).ToList();
            foreach (var generic in genericServices)
            {
                var selectService = new SelectListItem
                {
                    Text = generic.Manufacturer + " " + generic.Model + " " + generic.Name,
                    Value = generic.ServiceId.ToString()
                };

                applicableServices.Add(selectService);
            }

            invoiceModel.AvailableServices = applicableServices;

            return View(invoiceModel);
        }

        //this will be a method for JS, so send back a bool
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> Invoice(string orderNumber, string servId)
        {
            var workOrder = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == orderNumber);
            //make sure there is a workorder to connect to
            if (workOrder == null)
            {
                return false;
            }
            //if there are no services in db, return to a view that says this, can make work orders without them
            if (_db.Services.Any() == false)
            {
                return false;
            }

            int serviceid = Convert.ToInt32(servId);
            var service = _db.Services.FirstOrDefault(serv => serv.ServiceId == serviceid);
            if (service == null)
            {
                return false;
            }

            var joining = _db.WorkOrders.Where(ord => ord.WorkOrderNumber == orderNumber).
               SelectMany(ord => ord.WorkOrderServices).
               FirstOrDefault(wos => wos.WorkOrderNumber == orderNumber && wos.ServiceId == serviceid);

            if(joining == null)
            {
                workOrder.WorkOrderServices = new List<WorkOrderService>
                {
                    new WorkOrderService {
                        WorkOrder = workOrder,
                        WorkOrderNumber = workOrder.WorkOrderNumber,
                        Service = service,
                        ServiceId = service.ServiceId,
                        NumberOf = 1
                    }
                };
            }
            else
            {
                joining.NumberOf++;
            }
             
           
            _db.SaveChanges();

            // grab the Application User by the current user's username so UserId can be grabbed
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            

            //fill out the History fields that are common for all connected to a device
            var HistoryEntry = new WorkOrderHistory
            {
                UserId = user.Id,
                Username = User.Identity.Name,
                WorkOrderNumber = orderNumber,
                HistoryID = DateTime.Now.ToString(numberFormat + "fff"),
                TimeOfAction = DateTime.Now.ToString(timeFormat),
                ActionTaken = "Added Service",
                FieldValueAfter = service.Name + " $" + service.Price.ToString()
            };

            _db.Histories.Add(HistoryEntry);
            _db.SaveChanges();
                       
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveService(string orderNumber, string servId)
        {
            var workOrder = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == orderNumber);
            //make sure there is a workorder to connect to
            if (workOrder == null)
            {
                return RedirectToAction(nameof(List));
            }
            
            int serviceid = Convert.ToInt32(servId);
            var service = _db.Services.FirstOrDefault(serv => serv.ServiceId == serviceid);
            if (service == null)
            {
                return RedirectToAction("Invoice", "WorkOrder", new { orderNumber = orderNumber });
            }


            var joining = _db.WorkOrders.Where(ord => ord.WorkOrderNumber == orderNumber).
                SelectMany(ord => ord.WorkOrderServices).
                FirstOrDefault(wos => wos.WorkOrderNumber == orderNumber && wos.ServiceId == serviceid);
            if (joining.NumberOf > 1)
            {
                joining.NumberOf--;
            }
            else
            {
                _db.Remove(joining);
            }
            
            _db.SaveChanges();

            // grab the Application User by the current user's username so UserId can be grabbed
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            //fill out the History fields that are common for all connected to a device
            var HistoryEntry = new WorkOrderHistory
            {
                UserId = user.Id,
                Username = User.Identity.Name,
                WorkOrderNumber = orderNumber,
                HistoryID = DateTime.Now.ToString(numberFormat + "fff"),
                TimeOfAction = DateTime.Now.ToString(timeFormat),
                ActionTaken = "Removed Service",
                FieldValueAfter = service.Name + " $" + service.Price.ToString()
            };

            _db.Histories.Add(HistoryEntry);
            _db.SaveChanges();

            return RedirectToAction("Invoice", "WorkOrder", new { orderNumber = orderNumber });

        }

        // This should not be an action that happens often at all, will only be admin and should be accessed differently
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string orderNumber)
        {
            //find work order to delete
            var toDelete = _db.WorkOrders.FirstOrDefault(o => o.WorkOrderNumber == orderNumber);
            if (toDelete == null)
            {
                return RedirectToAction(nameof(List));
            }

            //delete all associated devices, should always be one but an admin may delete all devices from a work
            //order for some reason so just a check
            if (_db.Devices.Any(d => d.WorkOrderNumber == toDelete.WorkOrderNumber))
            {
                var linkedDevices = _db.Devices.Where(d => d.WorkOrderNumber == toDelete.WorkOrderNumber);
                foreach (var device in linkedDevices)
                {
                    _db.Remove(device);
                }
            }

            //on deletion of work orders, delete all History that is linked to it, check for security reason only
            if (_db.Histories.Any(his => his.WorkOrderNumber == toDelete.WorkOrderNumber))
            {
                var linkedHistory = _db.Histories.Where(his => his.WorkOrderNumber == toDelete.WorkOrderNumber);
                foreach (var history in linkedHistory)
                {
                    _db.Remove(history);
                }
            }

            _db.Remove(toDelete);
            _db.SaveChanges();

            return RedirectToAction(nameof(List));
            
        }
                
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async  Task<IActionResult> DeleteDevice(string deviceNumber)
        {
            //find device to delete
            var toDelete = _db.Devices.FirstOrDefault(o => o.DeviceId == deviceNumber);
            if (toDelete == null)
            {
                return RedirectToAction(nameof(List));
            }

            //creation of History with the generic function and filling in the applicable details after
            var deletionEntry = await GenerateBaseHistoryForDeviceWithoutUpdates(toDelete, deviceNumber);
            deletionEntry.ActionTaken = "DELETED DEVICE";
            deletionEntry.TimeOfAction = DateTime.Now.ToString(timeFormat);
            deletionEntry.HistoryID = DateTime.Now.ToString(numberFormat + "fff");

            _db.Histories.Add(deletionEntry);
            _db.Remove(toDelete);
            _db.SaveChanges();

            return RedirectToAction("Details", "WorkOrder", new {order = toDelete.WorkOrderNumber });

        }

        private async Task<WorkOrderHistory> GenerateBaseHistoryOnUpdation(Device updatedDevice)
        {
            // grab the Application User by the current user's username so UserId can be grabbed
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            //creation of a device identity that can be easily read in style of Details
            var deviceIdentity = updatedDevice.Manufacturer + " " + updatedDevice.Model + " SN: " + updatedDevice.Serial;

            //fill out the History fields that are common for all connected to a device
            var HistoryEntry = new WorkOrderHistory
            {
                UserId = user.Id,
                Username = User.Identity.Name,
                DeviceID = updatedDevice.DeviceId,
                DeviceIdentity = deviceIdentity,
                WorkOrderNumber = updatedDevice.WorkOrderNumber,
                HistoryID = DateTime.Now.ToString(numberFormat + "fff"),
                TimeOfAction = DateTime.Now.ToString(timeFormat)
            };
            //added fffffff to this because computers are fast so to differentiate histories in situations when multiple entities
            //are changed format to milliseconds

            return HistoryEntry;
        }

        private async Task<WorkOrderHistory> GenerateBaseHistoryForDeviceWithoutUpdates(Device device, string deviceNumber)
        {
            
            // grab the Application User by the current user's username so UserId can be grabbed
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            //creation of a device identity that can be easily read in style of Details
            var deviceIdentity = device.Manufacturer + " " + device.Model + " SN: " + device.Serial;

            //fill out the History fields that are common for all connected to a device
            var HistoryEntry = new WorkOrderHistory
            {
                UserId = user.Id,
                Username = User.Identity.Name,
                DeviceID = deviceNumber,
                DeviceIdentity = deviceIdentity,
                WorkOrderNumber=device.WorkOrderNumber
            };

            return HistoryEntry;
        }

        private List<SelectListItem> GenerateManufacturerList()
        {
            var manufacturers = _db.Manufacturers.OrderBy(manu => manu.ManufacturerName).ToList();
            var manufacturerList = new List<SelectListItem>();

            foreach (var manufacturer in manufacturers)
            {
                var listItem = new SelectListItem
                { Value=manufacturer.ManufacturerName, Text=manufacturer.ManufacturerName  };
                manufacturerList.Add(listItem);
            }

            return manufacturerList;
        }

        private List<SelectListItem> GenerateModelList(string manufacturer = null)
        {
            var models = new List<ModelofDevice>();
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
                models = _db.DeviceModels.Where(mod => mod.ManufacturerId == manufacturerId).OrderBy(mod => mod.ManufacturerId ).ToList();
            }
           
            var modelList = new List<SelectListItem>();

            foreach (var model in models)
            {
                var listItem = new SelectListItem
                { Value= model.Model, Text = model.Model };
                modelList.Add(listItem);
            }
                        
            return modelList;
        }

        private Device GenerateDeviceFromViewModel(AddDeviceViewModel created, string orderNumber)
        {
            var createdDevice = new Device
            {
                DeviceId = DateTime.Now.ToString(numberFormat),
                Status = "Created",
                Manufacturer = created.Manufacturer,
                Model = created.Model,
                CreatedAt = DateTime.Now.ToString(timeFormat),
                Password = created.Password,
                Provider = created.Provider,
                Serial = created.Serial,
                Notes = created.Notes,
                Problem = created.Problem,
                StatusLastUpdatedAt = DateTime.Now.ToString(timeFormat),
                WorkOrderNumber = orderNumber
            };

            return createdDevice;
        }

        //action for AJAX  to call
        public string ReturnLinkedModels(string manufacturer)
        {           
            //find the manufacturer with the selected name and get its id
            var selectedManufacturerId = _db.Manufacturers.FirstOrDefault(manufact => manufact.ManufacturerName == manufacturer).ManufacturerId;

            //then find all models that are linked to that id
            var linkedModels = _db.DeviceModels.Where(mod => mod.ManufacturerId == selectedManufacturerId);

            //create a list of strings to hold on to the names, empty so that foreach loop can add
            List<string> listOfModels = new List<string>();
            foreach(var linked in linkedModels)
            {                
                listOfModels.Add(linked.Model);
            }
            
            //convert list to model, so that Json has a models key (better format imo)
            var models = new ModelNames { Models = listOfModels };
            return JsonConvert.SerializeObject(models);
            
        }
    }
}