using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChatSystem.Models;
using ChatSystem.BL;
using ChatSystem.Helping_Clesses;

namespace ChatSystem.Controllers
{
    [ValidationFilter]
    public class CompanyController : Controller
    {
        private readonly GeneralPurpose gp = new GeneralPurpose();
        private readonly DatabaseEntities de = new DatabaseEntities();

        public ActionResult Index(string msg = "", string color = "black")
        {
            string BaseUrl = string.Format("{0}://{1}{2}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority, "/");
            int agentCount = new UserBL().GetActiveUserList(de).Where(x => x.CompanyId == gp.ValidateLoggedinUser().CompanyId && x.Role == 3).Count();
            int clientCount = new UserBL().GetActiveUserList(de).Where(x => x.CompanyId == gp.ValidateLoggedinUser().CompanyId && x.Role == 4).Count();

            ViewBag.AgentCount = agentCount;
            ViewBag.ClientCount = clientCount;
            ViewBag.InviteLink = BaseUrl + "Agent/ClientForm?cId=" + StringCipher.Base64Encode(gp.ValidateLoggedinUser().CompanyId.ToString());
            
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        #region Manage User

        public ActionResult AddUser(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        public ActionResult PostAddUser(User _user)
        {
            bool checkEmail = gp.ValidateEmailWithCompany(_user.Email, gp.ValidateLoggedinUser().Company.Name);
            if (checkEmail == false)
            {
                return RedirectToAction("AddUser", "Company", new { msg = "Email already exists in this company. Try another!", color = "red" });
            }

            User u = new User()
            {
                CompanyId = gp.ValidateLoggedinUser().CompanyId,
                Name = _user.Name.Trim(),
                Contact = _user.Contact,
                Address = _user.Address.Trim(),
                Email = _user.Email.Trim(),
                Password = StringCipher.Encrypt(_user.Password),
                IsEngaged = 0,
                Role = 3,
                IsActive = 1,
                CreatedAt = DateTime.Now
            };

            bool chkUser = new UserBL().AddUser(u, de);

            if (chkUser)
            {
                return RedirectToAction("AddUser", "Company", new { msg = "User created successfully", color = "green" });
            }
            else
            {
                return RedirectToAction("AddUser", "Company", new { msg = "Somethings' wrong", color = "red" });
            }
        }

        public ActionResult ViewUser(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        public ActionResult PostUpdateUser(User _user)
        {
            bool checkEmail = gp.ValidateEmailWithCompany(_user.Email, gp.ValidateLoggedinUser().Company.Name, _user.Id);
            if (checkEmail == false)
            {
                return RedirectToAction("ViewUser", "Company", new { msg = "Email already exists. Try another!", color = "red" });
            }

            User u = new UserBL().GetUserById(_user.Id, de);
            u.Name = _user.Name.Trim();
            u.Contact = _user.Contact;
            u.Address = _user.Address.Trim();
            u.Email = _user.Email.Trim();
            
            bool chkUser = new UserBL().UpdateUser(u, de);

            if (chkUser)
            {
                return RedirectToAction("ViewUser", "Company", new { msg = "User updated successfully", color = "green" });
            }
            else
            {
                return RedirectToAction("ViewUser", "Company", new { msg = "Somethings' wrong", color = "red" });
            }
        }

        public ActionResult DeleteUser(int _id)
        {

            User u = new UserBL().GetUserById(_id, de);
            u.IsActive = 0;


            bool chkUser = new UserBL().UpdateUser(u, de);

            if (chkUser)
            {
                return RedirectToAction("ViewUser", "Company", new { msg = "User deleted successfully", color = "green" });
            }
            else
            {
                return RedirectToAction("ViewUser", "Company", new { msg = "Somethings' wrong", color = "red" });
            }
        }
        #endregion


        #region AJAX

        [HttpPost]
        public ActionResult GetUserList(string name = "", string contact = "", string email = "")
        {
            List<User> ulist = new UserBL().GetActiveUserListByCompanyId((int)gp.ValidateLoggedinUser().CompanyId ,de).Where(x => x.Role == 3).OrderByDescending(x => x.CreatedAt).ToList();

            if (name != "")
            {
                ulist = ulist.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();
            }
            if (contact != "")
            {
                ulist = ulist.Where(x => x.Contact.ToLower().Contains(contact.ToLower())).ToList();
            }
            if (email != "")
            {
                ulist = ulist.Where(x => x.Email.ToLower().Contains(email.ToLower())).ToList();
            }

            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];


            int totalrows = ulist.Count();

            if (!string.IsNullOrEmpty(searchValue))
            {
                ulist = ulist.Where(x => x.Address.ToLower().Contains(searchValue.ToLower())).ToList();
            }

            int totalrowsafterfilterinig = ulist.Count();


            // pagination
            ulist = ulist.Skip(start).Take(length).ToList();

            List<User> udto = new List<User>();


            foreach (User u in ulist)
            {
                User obj = new User()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Contact = u.Contact,
                    Email = u.Email,
                    Address = u.Address,
                };

                udto.Add(obj);
            }

            return Json(new { data = udto, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetUserById(int id)
        {
            User u = new UserBL().GetUserById(id, de);


            User obj = new User()
            {
                Id = u.Id,
                Name = u.Name,
                Contact = u.Contact,
                Address = u.Address,
                Email = u.Email
            };

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ValidateUserEmail(string email, int id=-1)
        {
            bool chkUser = true;
            if (id != -1)
            {
                chkUser = gp.ValidateEmail(email, id);
            }
            else
            {
                chkUser = gp.ValidateEmail(email);
            }

            return Json(chkUser, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}