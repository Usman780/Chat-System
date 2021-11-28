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
using Microsoft.AspNet.SignalR;
using ChatSystem.DataHub;

namespace ChatSystem.Controllers
{
    public class ClientSupportController : Controller
    {
        DatabaseEntities de = new DatabaseEntities();
        GeneralPurpose gp = new GeneralPurpose();


        public ActionResult Test()
        {
            return View();
        }


        public ActionResult CheckChatSupport(string company, int type)
        {
            if (type == 1)
            {
                if (gp.ValidateLoggedinUser() != null)
                {
                    return RedirectToAction("ChatSupportHeader", "Agent", new { companyName = company, name = gp.ValidateLoggedinUser().Name, email = gp.ValidateLoggedinUser().Email, password = StringCipher.Decrypt(gp.ValidateLoggedinUser().Password) });
                }
                else
                {
                    return RedirectToAction("ChatSupportHeaderDefault", "Error");
                }
            }
            else
            {
                if (gp.ValidateLoggedinUser() != null)
                {
                    return RedirectToAction("ClientFormSupport", "Agent", new { companyName = company, name = gp.ValidateLoggedinUser().Name, email = gp.ValidateLoggedinUser().Email, password = StringCipher.Decrypt(gp.ValidateLoggedinUser().Password) });
                }
                else
                {
                    return RedirectToAction("Register", "ClientSupport", new { company = company});
                }
            }
        }


        public ActionResult Register(string company)
        {
            if (gp.ValidateLoggedinUser() != null)
            {
                return RedirectToAction("ClientFormSupport", "Agent", new { companyName = company, name = gp.ValidateLoggedinUser().Name, email = gp.ValidateLoggedinUser().Email, password = StringCipher.Decrypt(gp.ValidateLoggedinUser().Password) });
            }
            else
            {
                ViewBag.company = company;

                return View();
            }

        }

        #region AJAX

        [HttpPost]
        public ActionResult PostRegister(int isLogin, string company="", string email="", string password="", string name="")
        {
            if (isLogin == 0)
            {
                if (company == "" || email == "" || password == "" || name == "")
                {
                    return Json("All fields are required.", JsonRequestBehavior.AllowGet);
                }

                User user = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == company.Trim().ToLower() && x.Email.Trim().ToLower() == email.Trim().ToLower()).FirstOrDefault();
                if (user != null)
                {
                    return Json("Already registered, Please Login", JsonRequestBehavior.AllowGet);
                }

                Company comp = new CompanyBL().GetCompanyByName(company.Trim(), de);

                User obj = new User()
                {
                    CompanyId = comp.Id,
                    Name = name.Trim(),
                    Contact = null,
                    Address = null,
                    Email = email.Trim(),
                    Password = StringCipher.Encrypt(password),
                    IsEngaged = 0,
                    Role = 4,
                    IsActive = 1,
                    CreatedAt = DateTime.Now
                };

                bool chkUser = new UserBL().AddUser(obj, de);

                if (chkUser)
                {
                    if(SetCredentials(obj))
                    {
                        return Json("1", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Somethings' Wrong, Try login", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Somethings' Wrong", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {

                if (company == "" || email == "" || password == "")
                {
                    return Json("All fields are required.", JsonRequestBehavior.AllowGet);
                }

                User user = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == company.Trim().ToLower() && x.Email.Trim().ToLower() == email.Trim().ToLower() && StringCipher.Decrypt(x.Password).Equals(password)).FirstOrDefault();
                if (user == null)
                {
                    return Json("Incorrect credentials!", JsonRequestBehavior.AllowGet);
                }

                if (SetCredentials(user))
                {
                    return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Somethings' Wrong", JsonRequestBehavior.AllowGet);
                }

            }
        }

        #endregion

        public bool SetCredentials(User user)
        {
            try
            {
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
                bool chkUser2 = new UserBL().UpdateUser(user, de);

                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.All.broadcastUserStatus("1", user.Id, user.CompanyId);

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}