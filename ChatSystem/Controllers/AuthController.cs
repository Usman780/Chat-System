using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChatSystem.Models;
using ChatSystem.BL;
using ChatSystem.Helping_Clesses;
using System.Security.Claims;
using System.Threading;
using ChatSystem.DataHub;
using Microsoft.AspNet.SignalR;

namespace ChatSystem.Controllers
{
    [ValidationFilter(CheckLogin = false)] //filter class 
    public class AuthController : Controller
    {
        private readonly GeneralPurpose gp = new GeneralPurpose();
        private readonly DatabaseEntities de = new DatabaseEntities();

        
        public ActionResult Login(string msg = "", string color = "black")
        {
            if (gp.ValidateLoggedinUser() != null)
            {
                if (gp.ValidateLoggedinUser().Role == 2)
                {
                    return RedirectToAction("Index", "Company");
                }
                else if (gp.ValidateLoggedinUser().Role == 3)
                {
                    return RedirectToAction("Index", "Agent");
                }
                else if (gp.ValidateLoggedinUser().Role == 4)
                {
                    return RedirectToAction("Index", "Client");
                }
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        public ActionResult PostLogin(string Email = "", string Password = "", string _companyName="", string way="")
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

            User user = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == _companyName.Trim().ToLower() && x.Email.Trim().ToLower() == Email.Trim().ToLower() && StringCipher.Decrypt(x.Password).Equals(Password)).FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("Login", new { msg = "Incorrect credentials!", color = "red" });
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name,user.Id.ToString()),//id used for SignalR
                new Claim(ClaimTypes.Sid,user.Id.ToString()),
                new Claim("UserName", user.Name),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Role", user.Role.ToString()),

            }, "ApplicationCookie");

            var claimsPrincipal = new ClaimsPrincipal(identity); // Set current principal
            Thread.CurrentPrincipal = claimsPrincipal;
            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;

            authManager.SignIn(identity);

            user.IsLive = 1;
            bool chkUser = new UserBL().UpdateUser(user, de);
            context.Clients.All.broadcastUserStatus("1", user.Id, user.CompanyId);

            if (user.Role == 2)
            {
                return RedirectToAction("Index", "Company");
            }
            else if (user.Role == 3)
            {
                return RedirectToAction("Index", "Agent");
            }
            else
            {
                if (way == "Support")
                {
                    return RedirectToAction("ChatSupport", "Client");
                }
                else
                {
                    return RedirectToAction("Index", "Client");
                }
            }
        }


        #region Signup
        public ActionResult Register(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        public ActionResult PostRegister(User _user, string _companyName, string _confirmPassword = "")
        {
            
            if (_user.Password != _confirmPassword)
            {
                return RedirectToAction("Register", "Auth", new { msg = "Password and confirm password didn't match", color = "red" });
            }
            
            Company company = new CompanyBL().GetCompanyByName(_companyName.Trim(), de);
            if(company != null)
            {
                return RedirectToAction("Register", "Auth", new { msg = "Company name already exists. Try another name!", color = "red" });
            }

            bool checkEmail = gp.ValidateEmailWithCompany(_user.Email, _companyName);
            if (checkEmail == false)
            {
                return RedirectToAction("Register", "Auth", new { msg = "Email already exists in this Company. Try sign in!", color = "red" });
            }

            Company c = new Company()
            {
                Name = _companyName.Trim(),
                Address = _user.Address.Trim(),
                Email = _user.Email.Trim(),
                Contact = _user.Contact,
                NavbarColor = "#7462d0",
                ButtonColor = "#3fca5b",
                OtherColor = "#ff0000",
                IsActive = 1,
                CreatedAt = DateTime.Now
            };

            int CompanyId = new CompanyBL().AddCompany(c, de);

            if (CompanyId != -1)
            {
                User u = new User()
                {
                    CompanyId = CompanyId,
                    Name = _user.Name.Trim(),
                    Contact = _user.Contact,
                    Address = _user.Address.Trim(),
                    Email = _user.Email.Trim(),
                    Password = StringCipher.Encrypt(_user.Password),
                    IsEngaged = 0,
                    Role = 2,
                    IsActive = 1,
                    CreatedAt = DateTime.Now
                };

                bool chkUser = new UserBL().AddUser(u, de);

                if (chkUser)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Account created successfully, Please login", color="green" });
                }
                else
                {
                    bool chkCompany = new CompanyBL().DeleteCompany(CompanyId, de);

                    return RedirectToAction("Register", "Auth", new { msg = "Somethings' wrong", color = "red" });
                }
            }
            else
            {
                return RedirectToAction("Register", "Auth", new { msg = "Somethings' Wrong!", color = "red" });
            }

        }
        #endregion


        #region Forgot Password
        public ActionResult ForgotPassword(string msg = "", string color = "black")
        {
            ViewBag.Color = color;
            ViewBag.Message = msg;

            return View();
        }

        public ActionResult PostForgotPassword(string _companyName = "", string Email = "")
        {
            bool checkEmail = gp.ValidateEmailWithCompany(Email, _companyName);

            if (checkEmail == false)
            {
                string BaseUrl = string.Format("{0}://{1}{2}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority, "/");
            
                User user = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == _companyName.ToLower().Trim() && x.Email.ToLower() == Email.ToLower().Trim()).FirstOrDefault();

                bool checkMail = MailSender.SendForgotPasswordEmail(user.Id, user.Email, BaseUrl);

                if (checkMail == true)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Please check your inbox/spam", color = "green" });
                }
                else
                {
                    return RedirectToAction("ForgotPassword", "Auth", new { msg = "Mail sending fail!", color = "red" });
                }
            }
            else
            {
                return RedirectToAction("ForgotPassword", "Auth", new { msg = "Email does not belong to any record!!", color = "red" });
            }

        }


        public ActionResult ResetPassword(string id = "", string time = "", string msg = "", string color = "black")
        {
            DateTime PassDate = Convert.ToDateTime(StringCipher.Base64Decode(time)).Date;
            DateTime CurrentDate = DateTime.Now.Date;

            if (CurrentDate != PassDate)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Link expired, Please try again!", color = "red" });
            }


            ViewBag.Time = time;
            ViewBag.Id = id;

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [HttpPost]
        public ActionResult PostResetPassword(string Id = "", string Time = "", string NewPassword = "", string ConfirmPassword = "")
        {
            if (NewPassword != ConfirmPassword)
            {
                return RedirectToAction("ResetPassword", "Auth", new { id = Id, time = Time, msg = "Password and confirm password did not match", color = "red" });
            }

            int DecryptId = Convert.ToInt32(StringCipher.Base64Decode(Id));

            User user = new UserBL().GetUserById(DecryptId, de);

            user.Password = StringCipher.Encrypt(NewPassword);

            bool check = new UserBL().UpdateUser(user, de);

            if (check == true)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Password reset successful, Try login", color = "green" });
            }
            else
            {
                return RedirectToAction("ResetPassword", "Auth", new { id = Id, time = Time, msg = "Somethings' Wrong!", color = "red" });
            }
        }

        #endregion


        #region Manage User Profile

        [ValidationFilter(CheckLogin = true)]
        public ActionResult UpdateTheme(string msg = "", string color = "black")
        {
            Company c = new CompanyBL().GetCompanyById((int)gp.ValidateLoggedinUser().CompanyId, de);

            ViewBag.Company = c;

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [ValidationFilter(CheckLogin = true)]
        public ActionResult PostUpdateTheme(Company _company)
        {
            Company c = new CompanyBL().GetCompanyById(_company.Id, de);
            c.NavbarColor = _company.NavbarColor;
            c.ButtonColor = _company.ButtonColor;
            c.OtherColor = _company.OtherColor;


            bool chkCompany = new CompanyBL().UpdateCompany(c, de);

            if (chkCompany == true)
            {
                return RedirectToAction("UpdateTheme", "Auth", new { msg = "Theme updated successfully!", color = "green" });
            }
            else
            {
                return RedirectToAction("UpdateTheme", "Auth", new { msg = "Somthings' Wrong!", color = "red" });
            }
        }



        [ValidationFilter(CheckLogin = true)]
        public ActionResult UpdateUserPassword(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [ValidationFilter(CheckLogin = true)]
        public ActionResult PostUpdateUserPassword(string oldPassword = "", string newPassword = "", string confirmPassword = "")
        {
            if (newPassword != confirmPassword)
            {
                return RedirectToAction("UpdateUserPassword", "Auth", new { msg = "New password and Confirm password did not match!", color = "red" });
            }

            User u = new UserBL().GetUserById(gp.ValidateLoggedinUser().Id, de);

            if (StringCipher.Decrypt(u.Password) != oldPassword)
            {
                return RedirectToAction("UpdateUserPassword", "Auth", new { msg = "Old password did not match!", color = "red" });
            }

            u.Password = StringCipher.Encrypt(newPassword);

            bool chkUser = new UserBL().UpdateUser(u, de);

            if (chkUser == true)
            {
                return RedirectToAction("UpdateUserPassword", "Auth", new { msg = "Password updated successfully!", color = "green" });
            }
            else
            {
                return RedirectToAction("UpdateUserPassword", "Auth", new { msg = "Somthings' Wrong!", color = "red" });
            }
        }


        [ValidationFilter(CheckLogin = true)]
        public ActionResult UpdateProfile(string msg = "", string color = "black")
        {
            User u = new UserBL().GetUserById(gp.ValidateLoggedinUser().Id, de);

            ViewBag.User = u;

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [ValidationFilter(CheckLogin = true)]
        public ActionResult PostUpdateProfile(string EncId, User _user, string _companyName="")
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            
            int UserId = Convert.ToInt32(StringCipher.Decrypt(EncId));
            User u = new UserBL().GetUserById(UserId, de);
            
            

            if (_companyName != "")
            {
                Company company = new CompanyBL().GetActiveCompanyList(de).Where(x => x.Name.ToLower() == _companyName.ToLower().Trim() && x.Id != u.CompanyId).FirstOrDefault();
                if (company != null)
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Company name already exists, Please try another", color = "red" });
                }

                if (gp.ValidateEmailWithCompany(_user.Email, _companyName, UserId) == false)
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Email used by someone else in this company, Please try another", color = "red" });
                }

                Company c = new CompanyBL().GetCompanyById((int)u.CompanyId, de);
                c.Name = _companyName.Trim();
                c.Address = _user.Address.Trim();
                c.Email = _user.Email.Trim();
                c.Contact = _user.Contact;

                bool chkCompany = new CompanyBL().UpdateCompany(c, de);

                if (chkCompany)
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Somthings' Wrong!", color = "red" });
                }
            }
            else
            {
                if (gp.ValidateEmailWithCompany(_user.Email, u.Company.Name, UserId) == false)
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Email used by someone else, Please try another", color = "red" });
                }
            }


            u.Name = _user.Name.Trim();
            u.Contact = _user.Contact.Trim();
            u.Address = _user.Address.Trim();
            u.Email = _user.Email.Trim();

            bool chkUser = new UserBL().UpdateUser(u, de);

            if (chkUser == true)
            {
                ChatRoom cht = new ChatRoomBL().GetChatRoomByAgenttId(u.Id, de);
                if(cht != null)
                {
                    context.Clients.All.broadcastUpdateClientChatHead("1", gp.ValidateLoggedinUser().CompanyId, cht.Id);
                }

                return RedirectToAction("UpdateProfile", "Auth", new { msg = "Profile updated successfully!", color = "green" });
            }
            else
            {
                return RedirectToAction("UpdateProfile", "Auth", new { msg = "Somthings' Wrong!", color = "red" });
            }

        }
        #endregion


        public ActionResult Logout()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            User user = new UserBL().GetUserById(gp.ValidateLoggedinUser().Id, de);
            if(user != null)
            {
                user.IsLive = 0;
                bool check = new UserBL().UpdateUser(user, de);

            }

            context.Clients.All.broadcastUserStatus("1", user.Id, user.CompanyId);

            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;
            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Login");
        }

    }
}