using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Restaurant_Management_System_MVc.Models;
using Restaurant_Management_System_MVc.Respositories;
using Restaurant_Management_System_MVc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Restaurant_Management_System_MVc.Controllers
{
    public class HomeController : Controller
    {
        RestaurantDbEntities db;


        public HomeController()
        {
            db = new RestaurantDbEntities();
        }
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                TableRepository objCustomerRepository = new TableRepository();
                ItemRepository objItemRepository = new ItemRepository();
                PaymentTypeRepository objPaymentTypeRepository = new PaymentTypeRepository();
                var objMultipleModels = new Tuple<IEnumerable<SelectListItem>, IEnumerable<SelectListItem>, IEnumerable<SelectListItem>>
                    (objCustomerRepository.GetAllTables(), objItemRepository.GetAllItems(), objPaymentTypeRepository.GetAllPaymentType());
                return View(objMultipleModels);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }
        [HttpGet]
        public JsonResult getItemUnitPrice(int itemId)
        {
            decimal? unitPrice = db.Items.Single(model => model.ItemID == itemId).Price;
            return Json(unitPrice, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getCustomerOrder(int customerId)
        {
            OrderRepository orderRepository = new OrderRepository();
            OrderViewModel order =  orderRepository.GetCurrentOrderByCustomerId(customerId);

            return Json(order, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteOrderLine(string OrderNo, string ClientID, string ItemID, string Quantity, string Total)
        {
            OrderRepository orderRepository = new OrderRepository();
            orderRepository.DeleteOrderLine(OrderNo,Convert.ToInt32( ClientID),Convert.ToInt32( ItemID), Convert.ToDecimal(Quantity), Convert.ToDecimal(Total));

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PayOrder(string OrderNo, string ClientID, string PaymentTypeId)
        {
            OrderRepository orderRepository = new OrderRepository();
            orderRepository.PayOrder(OrderNo, Convert.ToInt32(ClientID), Convert.ToInt32(PaymentTypeId));

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "History of creation.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Khasanova Anzhelika.";

            return View();
        }
        [HttpPost]
        public JsonResult Index(OrderViewModel objOrderViewModel)
        {
           
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveOrderDetails(string ItemId, string UnitPrice, string Discount, string Total, string Quantity, string ClientID, string OrderNo)
        {

            OrderRepository orderRepository = new OrderRepository();

            var orderDetail = new OrderDetailViewModel()
            {
                ItemId = Convert.ToInt32(ItemId),
                Discount = Convert.ToDecimal(Discount),
                Quantity = Convert.ToDecimal(Quantity),
                Total = Convert.ToDecimal(Total),
                UnitPrice = Convert.ToDecimal(UnitPrice)
            };

            orderRepository.SaveOrderDetails(orderDetail, ClientID, OrderNo);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManageUsers()
        {
            //  UserRepository objUserRepository = new UserRepository();
            List<UserViewModel> users;
            using (var context = new ApplicationDbContext())
            {
                var usersWithRoles = (from user in context.Users
                                      select new
                                      {
                                          Id = user.Id,
                                          UserName = user.UserName,
                                          Email = user.Email,
                                          PhoneNumber = user.PhoneNumber,
                                          RoleNames = (from userRole in user.Roles
                                                       join role in context.Roles on userRole.RoleId
                                                       equals role.Id
                                                       select role.Name).ToList(),
                                                      
                                      }).ToList().Select(p => new UserInRoleViewModel()

                                      {
                                          Id = p.Id,
                                          UserName = p.UserName,
                                          Email = p.Email,
                                          PhoneNumber = p.PhoneNumber,
                                          Role = string.Join(",", p.RoleNames)
                                      });
                return View(usersWithRoles);

            }

        }


    }

    public class OrderRepository
    {
        RestaurantDbEntities db;
        public OrderRepository()
        {
            db = new RestaurantDbEntities();
        }


        public void SaveOrderDetails(OrderDetailViewModel orderDetailViewModel, string ClientID, string orderNo)
        {
            var orderItem = new OrderItem()
            {
                ItemID = orderDetailViewModel.ItemId,
                Quantity = Convert.ToInt32(orderDetailViewModel.Quantity)

            };
            var orderDet = new OrderDetail()
            {
                Discount = orderDetailViewModel.Discount,
                ItemId = orderDetailViewModel.ItemId,
                Quantity = orderDetailViewModel.Quantity,
                Total = orderDetailViewModel.Total,
                UnitPrice = orderDetailViewModel.UnitPrice,
            };

            if (string.IsNullOrWhiteSpace(orderNo))
            {
                var order = new Order()
                {
                    OrderNo = GetOrderNumber().ToString(),
                    GTotal = orderDetailViewModel.Total,
                    TableID = Convert.ToInt32(ClientID),
                    OrderDate = DateTime.Now
                };
                db.Orders.Add(order);
                order.OrderItems.Add(orderItem);
                order.OrderDetails.Add(orderDet);
                db.SaveChanges();
                return ;
            }
            else
            {
                var order = db.Orders.Where(o => o.OrderNo == orderNo).FirstOrDefault();
                order.OrderItems.Add(orderItem);
                order.OrderDetails.Add(orderDet);
                order.GTotal = Convert.ToDecimal(order.GTotal) + orderDet.Total;
                db.SaveChanges();
                
            }
        }

        public int GetOrderNumber()
        {
          return  db.Orders.Count() + 1;
        }

        public OrderViewModel GetCurrentOrderByCustomerId(int customerId) // customerId == tableId
        {
            var order = (from o in db.Orders
                         join t in db.Transactions
                         on o.OrderID equals t.OrderID
                           into j1

                         from t in j1.DefaultIfEmpty()
                         where o.TableID == customerId && t.OrderID != o.OrderID
                         select o
                          ).FirstOrDefault();
            if (order != null)
            {
                var details = new List<OrderDetailViewModel>();

                foreach (var det in order.OrderDetails.ToList())
                {
                    var itemName = db.Items.Where(i => i.ItemID == det.ItemId).FirstOrDefault().Name;
                    var detailsVM = new OrderDetailViewModel()
                    {
                        Discount = det.Discount,
                        ItemId = det.ItemId,
                        OrderDetailId = det.OrderDetailId,
                        Quantity = det.Quantity,
                        Total = det.Total,
                        UnitPrice = det.UnitPrice,
                        ItemName = itemName

                    };

                    details.Add(detailsVM);
                };

                var orderVM = new OrderViewModel()
                {
                    CustomerId = Convert.ToInt32(order.TableID),
                    OrderDate = Convert.ToDateTime(order.OrderDate),
                    FinalTotal = Convert.ToDecimal(order.GTotal),
                    OrderNumber = order.OrderNo,
                    ListOfOrderDetailViewModel = details
                };

                return orderVM;
            }
            else
                return new OrderViewModel();
        }

        public void DeleteOrderLine(string OrderNo, int ClientID, int ItemID, decimal Quantity, decimal Total)
        {
            var order = db.Orders.Where(o => o.OrderNo == OrderNo && o.TableID == ClientID).FirstOrDefault();
            var orderItem = order.OrderItems.Where(oi => oi.ItemID == ItemID && oi.Quantity == Quantity ).FirstOrDefault();
            var orderDetails = order.OrderDetails.Where(od => od.ItemId == ItemID && od.Quantity == Quantity && od.Total == Total).FirstOrDefault();

            order.GTotal = Convert.ToDecimal(order.GTotal) - orderDetails.Total;
            db.OrderItems.Remove(orderItem);
            db.OrderDetails.Remove(orderDetails);

            db.SaveChanges();
        }

        public void PayOrder(string OrderNo, int ClientID, int paymentTypeId)
        {
            var order = db.Orders.Where(o => o.OrderNo == OrderNo && o.TableID == ClientID).FirstOrDefault();
            PaymentType paymentType = db.PaymentTypes.Where(pt => pt.PaymentTypeId == paymentTypeId).FirstOrDefault();
            Transaction transaction = new Transaction()
            {
               //  PaymentType = paymentType,
                  TransactionDate = DateTime.Now,
                   Quantity = Convert.ToDecimal(order.GTotal),
                    TypeId = paymentTypeId
                      
            };

            order.Transactions.Add(transaction);
            db.SaveChanges();

        }
    }

    public  class UserViewModel
    {
        public UserViewModel()
    {
            Roles = new List<string>();
    }
    public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
    }
    internal class RoleViewModel
    {
        public string RoleID { get; set; }
        public string RoleName { get; set; }
    }
    public class UserInRoleViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }
}

