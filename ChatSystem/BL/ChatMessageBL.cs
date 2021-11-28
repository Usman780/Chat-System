using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;
using ChatSystem.DAL;

namespace ChatSystem.BL
{
    public class ChatMessageBL
    {
        public List<ChatMessage> GetAllChatMessageList(int _id, DatabaseEntities de)
        {
            return new ChatMessageDAL().GetAllChatMessageList(_id, de);
        }

        public List<ChatMessage> GetActiveChatMessageList(int _id, DatabaseEntities de)
        {
            return new ChatMessageDAL().GetActiveChatMessageList(_id, de);
        }

        public List<ChatMessage> GetUnreadChatMessageList(int _id, DatabaseEntities de)
        {
            return new ChatMessageDAL().GetUnreadChatMessageList(_id, de);
        }

        public ChatMessage GetChatMessageById(int _Id, DatabaseEntities de)
        {
            return new ChatMessageDAL().GetChatMessageById(_Id, de);
        }

        public int AddChatMessage(ChatMessage _ChatMessage, DatabaseEntities de)
        {
            return new ChatMessageDAL().AddChatMessage(_ChatMessage, de);
        }

        public bool UpdateChatMessage(ChatMessage _ChatMessage, DatabaseEntities de)
        {
            return new ChatMessageDAL().UpdateChatMessage(_ChatMessage, de);
        }

        public bool MarkUnreadChatMessage(int _id, DatabaseEntities de)
        {
            return new ChatMessageDAL().MarkUnreadChatMessage(_id, de);
        }

        public bool DeleteChatMessage(int _Id, DatabaseEntities de)
        {
            return new ChatMessageDAL().DeleteChatMessage(_Id, de);
        }

        public bool ClearUnreadMessage(int _Id, int _userId, DatabaseEntities de)
        {
            return new ChatMessageDAL().ClearUnreadMessage(_Id, _userId, de);
        }

    }
}