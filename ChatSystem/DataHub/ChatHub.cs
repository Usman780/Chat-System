using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;
using ChatSystem.BL;
using ChatSystem.Helping_Clesses;
using System.Threading.Tasks;

namespace ChatSystem.DataHub
{
    public class ChatHub : Hub
    {
        private readonly GeneralPurpose gp = new GeneralPurpose();
        private readonly DatabaseEntities de = new DatabaseEntities();

        public override Task OnConnected()
        {
            string id = Context.User.Identity.Name;

            User user = new UserBL().GetUserById(Convert.ToInt32(id), de);
            user.IsLive = 1;
            
            bool check = new UserBL().UpdateUser(user, de);

            if (check)
            {
                Clients.All.broadcastUserStatus("1", user.Id, user.CompanyId);
            }
            else
            {
                Clients.All.broadcastUserStatus("0", 0, 0);
            }

            return base.OnConnected();
        }


        public override Task OnReconnected()
        {
            string id = Context.User.Identity.Name;

            User user = new UserBL().GetUserById(Convert.ToInt32(id), de);
            user.IsLive = 1;

            bool check = new UserBL().UpdateUser(user, de);

            if (check)
            {
                Clients.All.broadcastUserStatus("1", user.Id, user.CompanyId);
            }
            else
            {
                Clients.All.broadcastUserStatus("0", 0, 0);
            }

            return base.OnReconnected();
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            string id = Context.User.Identity.Name;

            User user = new UserBL().GetUserById(Convert.ToInt32(id), de);
            user.IsLive = 0;

            bool check = new UserBL().UpdateUser(user, de);

            if (check)
            {
                Clients.All.broadcastUserStatus("1", user.Id, user.CompanyId);
            }
            else
            {
                Clients.All.broadcastUserStatus("0", 0, 0);
            }


            return Task.FromResult(0);
            //return base.OnDisconnected(true);
        }


        public void SendMessage(int chatId, int senderId, string message)
        {
            ChatRoom chat = new ChatRoomBL().GetChatRoomById(chatId, de);

            if(chat.IsActive == 2)
            {
                if (gp.ValidateLoggedinUser().Role == 4)
                {
                    Clients.All.broadcastClientSupportMessage("2", senderId, chatId, "", "", "", ""); //this function is used in Client/ChatSupport.cshtml
                    Clients.All.broadcastClientMessage("2", senderId, gp.ValidateLoggedinUser().CompanyId);
                    Clients.All.broadcastAgentMessage("2", chat.AgentId, gp.ValidateLoggedinUser().CompanyId);
                }
                else
                {
                    Clients.All.broadcastClientSupportMessage("2", senderId, chatId, "", "", "", ""); //this function is used in Client/ChatSupport.cshtml
                    Clients.All.broadcastAgentMessage("2", chat.AgentId, gp.ValidateLoggedinUser().CompanyId);
                    Clients.All.broadcastClientMessage("2", chat.ClientId, gp.ValidateLoggedinUser().CompanyId);
                }
            }

            ChatMessage obj = new ChatMessage()
            {
                CompanyId = gp.ValidateLoggedinUser().CompanyId,
                ChatRoomId = chat.Id,
                SenderId = senderId,
                Message = message.Trim(),
                IsRead = 0,
                IsActive = 1,
                CreatedAt = DateTime.Now
            };

            int MsgId = new ChatMessageBL().AddChatMessage(obj, de);


            if (MsgId != -1)
            {
                if(chat.AgentId == null)
                {
                    Clients.All.broadcastUpdateChatRequests("1", gp.ValidateLoggedinUser().CompanyId);
                }

                if (gp.ValidateLoggedinUser().Role == 4)
                {
                    Clients.All.broadcastClientSupportMessage("1", senderId, chatId, message, chat.User1.Name, Convert.ToDateTime(obj.CreatedAt).ToString("dddd, dd MMMM yyyy"), Convert.ToDateTime(obj.CreatedAt).ToString("HH:mm:ss")); //this function is used in Client/ChatSupport.cshtml
                    Clients.All.broadcastClientMessage("1", senderId, gp.ValidateLoggedinUser().CompanyId);
                    Clients.All.broadcastAgentMessage("1", chat.AgentId, gp.ValidateLoggedinUser().CompanyId);
                }
                else
                {
                    Clients.All.broadcastUpdateClientChatHead("1", gp.ValidateLoggedinUser().CompanyId, chatId); //this function is used in Client/ChatSupportHeader.cshtml
                    Clients.All.broadcastClientSupportMessage("1", senderId, chatId, message, chat.User.Name, Convert.ToDateTime(obj.CreatedAt).ToString("dddd, dd MMMM yyyy"), Convert.ToDateTime(obj.CreatedAt).ToString("HH:mm:ss")); //this function is used in Client/ChatSupport.cshtml
                    Clients.All.broadcastAgentMessage("1", chat.AgentId, gp.ValidateLoggedinUser().CompanyId);
                    Clients.All.broadcastClientMessage("1", chat.ClientId, gp.ValidateLoggedinUser().CompanyId);
                }
            }
            else
            {
                Clients.All.broadcastMessage("0", "null", "null");
                Clients.All.broadcastAgentMessage("0", "null", "null");
            }
        }


        public void SendTypingStatus(int status,int chatId, int from)
        {
            //if from == 1 then agent is typing and viseversa
            Clients.All.broadcastClientTypingStatus(status, chatId, from); // used in Client/Chatsupport.cshtml & Agent/ChatSupportHeader.cshtml
        }

    }
}