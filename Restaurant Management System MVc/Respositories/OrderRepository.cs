//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Restaurant_Management_System_MVc.Models;

//namespace Restaurant_Management_System_MVc.Respositories
//{
//    public class OrderRepository
//    {

//        RestaurantDbEntities db;
//        public OrderRepository()
//        {
//            db = new RestaurantDbEntities();
//        }
//        public IEnumerable<SelectListItem> GetAllItems()
//        {
//            IEnumerable<SelectListItem> objSelectListItems = new List<SelectListItem>();
//            objSelectListItems = (from obj in db.Items
//                                  select new SelectListItem()
//                                  {
//                                      Text = obj.Name,
//                                      Value = obj.ItemID.ToString(),
//                                      Selected = true

//                                  }).ToList();
//            return objSelectListItems;
//        }

//        //public void SaveOrder(OrderViewModel orderViewModel)
//        //{
//        //    db.Orders.Add(new Order()
//        //    {
//        //        OrderNo = "test",
//        //        GTotal = 100,
//        //        TableID = 4
//        //    });

//        //}
//    }
//}