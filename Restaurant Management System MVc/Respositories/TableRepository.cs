using Restaurant_Management_System_MVc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Restaurant_Management_System_MVc.Respositories
{
    public class TableRepository
    {
        RestaurantDbEntities db;

        public TableRepository()
        {
            db = new RestaurantDbEntities();
        }
        public IEnumerable<SelectListItem> GetAllTables()
        {
            IEnumerable<SelectListItem> objSelectListItems = new List<SelectListItem>();
            
            objSelectListItems = (from obj in db.Tables
                                  select new SelectListItem()
                                  {
                                      Text = obj.Name,
                                      Value = obj.TableID.ToString(),
                                      Selected = true

                                  }).ToList();
            return objSelectListItems;
        }
    }
}