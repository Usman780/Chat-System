using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChatSystem.Models;
using ChatSystem.BL;
using ChatSystem.Helping_Clesses;
using ChatSystem.DataHub;
using Microsoft.AspNet.SignalR;
using System.Security.Claims;
using System.Threading;

namespace ChatSystem.Controllers
{
    [ValidationFilter]
    public class AgentController : Controller
    {
        private readonly DatabaseEntities de = new DatabaseEntities();
        private readonly GeneralPurpose gp = new GeneralPurpose();

        public ActionResult Index(string msg = "", string color = "black")
        {
            ChatRoom cht = new ChatRoomBL().GetChatRoomByAgenttId(gp.ValidateLoggedinUser().Id, de);

            if (cht == null)
            {
                List<ChatRoom> chatList = new ChatRoomBL().GetActiveChatRoomList(de).Where(x => x.AgentId == null).ToList();

                List<User> msgList = new List<User>();

                foreach (ChatRoom c in chatList)
                {
                    int msgCount = c.ChatMessages.Count();
                    if (msgCount > 0)
                    {
                        User obj = new User()
                        {
                            Id = c.Id,
                            Name = c.User1.Name,
                            IsActive = msgCount
                        };

                        msgList.Add(obj);
                    }
                }

                ViewBag.MsgList = msgList;

                ViewBag.Message = msg;
                ViewBag.Color = color;

                return View();
            }
            else
            {
                return RedirectToAction("ClientChat", "Agent");
            }
        }

        [ValidationFilter(CheckLogin = false)]
        public ActionResult ClientForm(string cId="", string msg = "", string color = "black")
        {
            if (gp.ValidateLoggedinUser() != null)
            {
                return RedirectToAction("PostLogin", "Auth", new { Email = gp.ValidateLoggedinUser().Email, Password = StringCipher.Decrypt(gp.ValidateLoggedinUser().Password), _companyName = gp.ValidateLoggedinUser().Company.Name });
            }
            else
            {
                Company company = new CompanyBL().GetCompanyById(Convert.ToInt32(StringCipher.Base64Decode(cId)), de);

                ViewBag.CompanyId = cId;
                ViewBag.CompanyName = company.Name;
                ViewBag.Message = msg;
                ViewBag.Color = color;

                return View();
            }
        }

        [ValidationFilter(CheckLogin = false)]
        public ActionResult PostRegisterClient(User _user, string cId = "")
        {
            Company company = new CompanyBL().GetCompanyById(Convert.ToInt32(StringCipher.Base64Decode(cId)), de);

            bool checkEmail = gp.ValidateEmailWithCompany(_user.Email, company.Name);
            if (checkEmail == false)
            {
                return RedirectToAction("ClientForm", "Agent", new { cId= cId, msg = "Email already exists. Try sign in!", color = "red" });
            }

            User u = new User()
            {
                CompanyId = company.Id,
                Name = _user.Name.Trim(),
                Contact = _user.Contact,
                Address = _user.Address.Trim(),
                Email = _user.Email.Trim(),
                Password = StringCipher.Encrypt(_user.Password.Trim()),
                IsEngaged = 0,
                Role = 4,
                IsActive = 1,
                CreatedAt = DateTime.Now
            };

            int UserId = new UserBL().AddUser2(u, de);

            if (UserId != -1)
            {
                ChatRoom cr = new ChatRoom()
                {
                    CompanyId = company.Id,
                    ClientId = UserId,
                    IsClosed = 0,
                    IsActive = 1,
                    CreatedAt = DateTime.Now
                };

                int ChatId = new ChatRoomBL().AddChatRoom(cr, de);
                if(ChatId == -1)
                {
                    bool chkUser = new UserBL().DeleteUser(UserId, de);

                    return RedirectToAction("ClientForm", "Agent", new { cId = cId, msg = "Somethings' wrong", color = "red" });
                }
                return RedirectToAction("PostLogin", "Auth", new { Email = u.Email, Password = _user.Password, _companyName = company.Name });
            }
            else
            {
                return RedirectToAction("ClientForm", "Agent", new { cId = cId, msg = "Somethings' wrong", color = "red" });
            }
            

        }


        public ActionResult ClientChat(string msg = "", string color = "black")
        {
            ChatRoom cht = new ChatRoomBL().GetChatRoomByAgenttId(gp.ValidateLoggedinUser().Id, de);
            if (cht != null)
            {
                List<ChatMessage> msgList = new ChatMessageBL().GetActiveChatMessageList(cht.Id, de);

                ViewBag.Chat = cht;
                ViewBag.MsgList = msgList;

                ViewBag.Message = msg;
                ViewBag.Color = color;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Agent");
            }
        }


        #region Zuptu Support

        [ValidationFilter(CheckLogin = false)]
        public ActionResult Test()
        {
            return View();
        }


        [ValidationFilter(CheckLogin = false)]
        public ActionResult ChatSupportHeader(string companyName = "", string name = "", string email = "", string password = "")
        {
            try
            {
                Company company = new CompanyBL().GetActiveCompanyList(de).Where(x => x.Name.ToLower() == companyName.ToLower()).FirstOrDefault();
                if (company == null)
                {
                    return RedirectToAction("ChatSupportHeaderError", "Error", new { msg = "1" });
                }

                User user = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == companyName.ToLower() && x.Email.ToLower() == email.Trim().ToLower()).FirstOrDefault();

                if (user != null)
                {
                    ChatRoom cht = new ChatRoomBL().GetChatRoomByClientId(user.Id, de);

                    if (cht == null)
                    {
                        ChatRoom cr = new ChatRoom()
                        {
                            CompanyId = user.CompanyId,
                            ClientId = user.Id,
                            IsClosed = 0,
                            IsActive = 1,
                            CreatedAt = DateTime.Now
                        };

                        int ChatId = new ChatRoomBL().AddChatRoom(cr, de);

                        if (ChatId != -1)
                        {
                            cht = new ChatRoomBL().GetChatRoomById(ChatId, de);
                        }
                        else
                        {
                            return RedirectToAction("ChatSupportHeaderError", "Error", new { msg = "2" });
                        }
                    }

                    int msgCount = new ChatMessageBL().GetUnreadChatMessageList(cht.Id, de).Where(x => x.SenderId != user.Id).Count();

                    ViewBag.MsgCount = msgCount;
                    ViewBag.Chat = cht;

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

                    return View();
                }
                else
                {
                    return RedirectToAction("PostRegisterClientSupport", "Agent", new { CompanyName = companyName, Name = name, Email = email, Password = password, way = 2 });
                }
            }
            catch
            {
                return RedirectToAction("ChatSupportHeaderError", "Error", new { msg = "3" });
            }
        }


        [ValidationFilter(CheckLogin = false)]
        public ActionResult ClientFormSupport(string companyName = "",string name = "", string email = "", string password = "")
        {
            try
            {
                Company company = new CompanyBL().GetActiveCompanyList(de).Where(x => x.Name.ToLower() == companyName.ToLower()).FirstOrDefault();
                if (company == null)
                {
                    return RedirectToAction("ChatSupportError", "Error", new { msg = "1" });
                }

                //if(company == null)
                //{
                //    throw new Exception();
                //}

                if (gp.ValidateLoggedinUser() != null)
                {
                    //if(gp.ValidateLoggedinUser().Email.ToLower() == email.ToLower())
                    //{
                    //    return RedirectToAction("ChatSupport", "Client");
                    //}
                    //else
                    //{
                    var ctx = Request.GetOwinContext();
                    var authManager = ctx.Authentication;
                    authManager.SignOut("ApplicationCookie");
                    //}
                }

                //User user = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name == company.Name && x.Email.Trim().ToLower() == email.Trim().ToLower() && StringCipher.Decrypt(x.Password).Equals(password)).FirstOrDefault();
                User user = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == companyName.ToLower() && x.Email.ToLower() == email.Trim().ToLower()).FirstOrDefault();

                if (user != null)
                {
                    if (!StringCipher.Decrypt(user.Password).Equals(password))
                    {
                        user.Password = StringCipher.Encrypt(password);
                        bool chkUser = new UserBL().UpdateUser(user, de);
                    }

                    return RedirectToAction("PostLogin", "Auth", new { Email = user.Email, Password = password, _companyName = user.Company.Name, way = "Support" });
                }
                else
                {
                    return RedirectToAction("PostRegisterClientSupport", "Agent", new { CompanyName = companyName, Name = name, Email = email, Password = password });
                }
            }
            catch
            {
                return RedirectToAction("ChatSupportError", "Error", new { msg = "3" });
            }
        }

        [ValidationFilter(CheckLogin = false)]
        public ActionResult PostRegisterClientSupport(string CompanyName, User _user, int way=-1)
        {
            try
            {
                Company company = new CompanyBL().GetActiveCompanyList(de).Where(x => x.Name.ToLower() == CompanyName.ToLower()).FirstOrDefault();

                User u = new User()
                {
                    CompanyId = company.Id,
                    Name = _user.Name.Trim(),
                    Contact = null,
                    Address = null,
                    Email = _user.Email.Trim(),
                    Password = StringCipher.Encrypt(_user.Password.Trim()),
                    IsEngaged = 0,
                    Role = 4,
                    IsActive = 1,
                    CreatedAt = DateTime.Now
                };

                int UserId = new UserBL().AddUser2(u, de);

                if (UserId != -1)
                {
                    ChatRoom cr = new ChatRoom()
                    {
                        CompanyId = _user.CompanyId,
                        ClientId = UserId,
                        IsClosed = 0,
                        IsActive = 1,
                        CreatedAt = DateTime.Now
                    };

                    int ChatId = new ChatRoomBL().AddChatRoom(cr, de);
                    if (ChatId != -1)
                    {
                        if (way == 2)
                        {
                            return RedirectToAction("ChatSupportHeader", "Agent", new { companyName = u.Company.Name, name = u.Name, email = u.Email, password = StringCipher.Decrypt(u.Password) });
                        }
                        else
                        {
                            return RedirectToAction("PostLogin", "Auth", new { Email = u.Email, Password = _user.Password, _companyName = company.Name, way = "Support" });
                        }
                    }
                    else
                    {
                        bool chkUser = new UserBL().DeleteUser(UserId, de);

                        if (way == 2)
                        {
                            return RedirectToAction("ChatSupportHeaderError", "Error", new { msg = "2" });
                        }
                        else
                        {
                            return RedirectToAction("ChatSupportError", "Error", new { msg = "2" });
                        }

                    }
                }
                else
                {
                    if (way == 2)
                    {
                        return RedirectToAction("ChatSupportHeaderError", "Error", new { msg = "2" });
                    }
                    else
                    {
                        return RedirectToAction("ChatSupportError", "Error", new { msg = "2" });
                    }
                }
            }
            catch
            {
                if (way == 2)
                {
                    return RedirectToAction("ChatSupportHeaderError", "Error", new { msg = "3" });
                }
                else
                {
                    return RedirectToAction("ChatSupportError", "Error", new { msg = "3" });
                }
            }
        }
        #endregion


        #region AJAX

        [HttpPost]
        public ActionResult UpdateChatRequests(int companyId, int chatId, int agentId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

            ChatRoom chat = new ChatRoomBL().GetChatRoomById(chatId, de);
            chat.AgentId = agentId;
            chat.StartedAt = DateTime.Now;

            bool chkChat = new ChatRoomBL().UpdateChatRoom(chat, de);
            if(chkChat)
            {
                context.Clients.All.broadcastUpdateChatRequests("1", companyId);
                context.Clients.All.broadcastUpdateClientChatHead("1", companyId, chat.Id);

                return Json("1", JsonRequestBehavior.AllowGet);
            }
            else
            {
                context.Clients.All.broadcastUpdateChatRequests("0", null);
                context.Clients.All.broadcastUpdateClientChatHead("0", null, null);

                return Json("0", JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult CloseChat(int chatId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

            ChatRoom chat = new ChatRoomBL().GetChatRoomById(chatId, de);
            chat.ClosedAt = DateTime.Now;
            chat.IsClosed = 1;
            chat.IsActive = 2;

            bool chkChat = new ChatRoomBL().UpdateChatRoom(chat, de);
            if (chkChat)
            {
                bool checkMail = MailSender.SendChatStatus(chat.Id);

                context.Clients.All.broadcastClientSupportMessage("2", chat.ClientId, chat.Id, "", "", "", "");
                context.Clients.All.broadcastClientMessage("2", chat.ClientId, gp.ValidateLoggedinUser().CompanyId);
                context.Clients.All.broadcastAgentMessage("2", chat.AgentId, gp.ValidateLoggedinUser().CompanyId);

                return Json("1", JsonRequestBehavior.AllowGet);
            }
            else
            {
                context.Clients.All.broadcastUpdateChatRequests("0", null);
                context.Clients.All.broadcastUpdateClientChatHead("0", null, null);

                return Json("0", JsonRequestBehavior.AllowGet);
            }

        }

        #endregion
    }
}