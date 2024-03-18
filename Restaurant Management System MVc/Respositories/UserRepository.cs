using System;
using System.Collections.Generic;
using Restaurant_Management_System_MVc.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Restaurant_Management_System_MVc.ViewModel;

namespace Restaurant_Management_System_MVc.Respositories
{
    public class UserRepository
    {
        RestaurantDbEntities db;

        public UserRepository()
        {
            db = new RestaurantDbEntities();
        }
        public IEnumerable<UserViewModel> GetAllUsers()
        {
            //var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            //var authManager = HttpContext.GetOwinContext().Authentication;

            //ApplicationUser user = userManager.Find(model.Email, model.Password);
           // IEnumerable<SelectListItem> objSelectListItems = new List<SelectListItem>();

            var users = (from obj in db.AspNetUsers
                                  select new UserViewModel()
                                  { 
                                     

                                  }).ToList();
           // objSelectListItems = db.AspNetUsers.ToList();
            return users;
        }
    }
}
