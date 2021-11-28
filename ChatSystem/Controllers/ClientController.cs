using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChatSystem.Models;
using ChatSystem.BL;
using ChatSystem.Helping_Clesses;
using Microsoft.AspNet.SignalR;
using ChatSystem.DataHub;

namespace ChatSystem.Controllers
{
    [ValidationFilter]
    public class ClientController : Controller
    {
        private readonly GeneralPurpose gp = new GeneralPurpose();
        private readonly DatabaseEntities de = new DatabaseEntities();

        
        public ActionResult Index(string msg = "", string color = "black")
        {
            ChatRoom cht = new ChatRoomBL().GetChatRoomByClientId(gp.ValidateLoggedinUser().Id, de);
            List<ChatMessage> msgList = new List<ChatMessage>();

            if (cht == null)
            {
                return RedirectToAction("ChatHistory", "Client");
            }
            else
            {
                msgList = new ChatMessageBL().GetActiveChatMessageList(cht.Id, de);
            }

            ViewBag.Chat = cht;
            ViewBag.MsgList = msgList;

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        public ActionResult ChatHistory(string msg = "", string color = "black")
        {
            List<ChatRoom> cList = new ChatRoomBL().GetAllChatRoomList(de).Where(x => x.ClientId == gp.ValidateLoggedinUser().Id && x.IsActive == 2).ToList();
            if (cList.Count > 0)
            {
                ViewBag.ChatList = cList;

                ViewBag.Color = color;
                ViewBag.Message = msg;

                return View();
            }
            else
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        public ActionResult NewChat(string companyId = "")
        {
            ChatRoom cht = new ChatRoomBL().GetChatRoomByClientId(gp.ValidateLoggedinUser().Id, de);
            if (cht != null)
            {
                return RedirectToAction("Index", "Client");
            }
            else
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

                ChatRoom cr = new ChatRoom()
                {
                    CompanyId = Convert.ToInt32(StringCipher.Decrypt(companyId)),
                    ClientId = gp.ValidateLoggedinUser().Id,
                    IsClosed = 0,
                    IsActive = 1,
                    CreatedAt = DateTime.Now
                };

                int ChatId = new ChatRoomBL().AddChatRoom(cr, de);
                if (ChatId != -1)
                {
                    context.Clients.All.broadcastUpdateChatRequests("1", gp.ValidateLoggedinUser().CompanyId);

                    return RedirectToAction("Index", "Client");
                }
                else
                {
                    throw new Exception();
                    context.Clients.All.broadcastUpdateChatRequests("1", gp.ValidateLoggedinUser().CompanyId);
                    return RedirectToAction("ChatHistory", "Client", new { msg = "Somethings' wrong"});
                }
            }
        }


        #region Zuptu Chat
        [ValidationFilter(CheckLogin = false)]
        public ActionResult ChatSupport(string msg = "", string color = "black")
        {
            ChatRoom cht = new ChatRoomBL().GetChatRoomByClientId(gp.ValidateLoggedinUser().Id, de);
            List<ChatMessage> msgList = new List<ChatMessage>();

            if (cht == null)
            {
                return RedirectToAction("NewChatSupport", "Client", new { companyId = StringCipher.Encrypt(gp.ValidateLoggedinUser().CompanyId.ToString()) });
            }
            else
            {
                bool chkMsg = new ChatMessageBL().ClearUnreadMessage(cht.Id, gp.ValidateLoggedinUser().Id, de);
                msgList = new ChatMessageBL().GetActiveChatMessageList(cht.Id, de);
            }

            ViewBag.Chat = cht;
            ViewBag.MsgList = msgList.OrderBy(x=>x.Id).ToList();

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }
        
        [ValidationFilter(CheckLogin = false)]
        public ActionResult NewChatSupport(string companyId = "")
        {
            ChatRoom cht = new ChatRoomBL().GetChatRoomByClientId(gp.ValidateLoggedinUser().Id, de);
            if (cht != null)
            {
                return RedirectToAction("ChatSupport", "Client");
            }
            else
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

                ChatRoom cr = new ChatRoom()
                {
                    CompanyId = Convert.ToInt32(StringCipher.Decrypt(companyId)),
                    ClientId = gp.ValidateLoggedinUser().Id,
                    IsClosed = 0,
                    IsActive = 1,
                    CreatedAt = DateTime.Now
                };

                int ChatId = new ChatRoomBL().AddChatRoom(cr, de);
                if (ChatId != -1)
                {
                    context.Clients.All.broadcastUpdateChatRequests("1", gp.ValidateLoggedinUser().CompanyId);

                    return RedirectToAction("ChatSupport", "Client");
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        #endregion
        #region AJAX

        [HttpPost]
        public ActionResult GetChatList(string name = "")
        {
            List<ChatRoom> clist = new ChatRoomBL().GetAllChatRoomList(de).Where(x => x.User1.Id == gp.ValidateLoggedinUser().Id && x.IsActive == 2).OrderByDescending(x => x.ClosedAt).ToList();

            if (name != "")
            {
                clist = clist.Where(x => x.User.Name.ToLower().Contains(name.ToLower())).ToList();
            }
            

            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];


            int totalrows = clist.Count();

            if (!string.IsNullOrEmpty(searchValue))
            {
                clist = clist.Where(x => x.User.Name.ToLower().Contains(searchValue.ToLower())).ToList();
            }

            int totalrowsafterfilterinig = clist.Count();


            // pagination
            clist = clist.Skip(start).Take(length).ToList();

            List<ChatDTO> cdto = new List<ChatDTO>();


            foreach (ChatRoom c in clist)
            {
                ChatDTO obj = new ChatDTO()
                {
                    Id = c.Id,
                    AgentName = c.User.Name,
                    StartedAt = Convert.ToDateTime(c.StartedAt).ToString("MM/dd/yyyy hh:mm tt"),
                    ClosedAt = Convert.ToDateTime(c.ClosedAt).ToString("MM/dd/yyyy hh:mm tt"),
                    TotalMessages = c.ChatMessages.Count
                };

                cdto.Add(obj);
            }

            return Json(new { data = cdto, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetClearUnreadMsg(int chatId)
        {
            bool chkMsg = new ChatMessageBL().ClearUnreadMessage(chatId, gp.ValidateLoggedinUser().Id, de);

            return Json(1, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}