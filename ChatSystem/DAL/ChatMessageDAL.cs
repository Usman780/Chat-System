using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;

namespace ChatSystem.DAL
{
    public class ChatMessageDAL
    {

        public List<ChatMessage> GetAllChatMessageList(int _id, DatabaseEntities de)
        {
            return de.ChatMessages.Where(x=>x.ChatRoomId == _id).ToList();
        }

        public List<ChatMessage> GetActiveChatMessageList(int _id, DatabaseEntities de)
        {
            return de.ChatMessages.Where(x => x.ChatRoomId == _id && x.IsActive == 1).OrderByDescending(x=> x.CreatedAt).ToList();
        }

        public List<ChatMessage> GetUnreadChatMessageList(int _id, DatabaseEntities de)
        {
            return de.ChatMessages.Where(x => x.ChatRoomId == _id && x.IsRead == 0 && x.IsActive == 1).ToList();
        }

        public ChatMessage GetChatMessageById(int _Id, DatabaseEntities de)
        {
            return de.ChatMessages.Where(x => x.Id == _Id).FirstOrDefault(x => x.IsActive == 1);
        }

        public int AddChatMessage(ChatMessage _ChatMessage, DatabaseEntities de)
        {
            try
            {
                de.ChatMessages.Add(_ChatMessage);
                de.SaveChanges();

                return _ChatMessage.Id;
            }
            catch
            {
                return -1;
            }
        }

        public bool UpdateChatMessage(ChatMessage _ChatMessage, DatabaseEntities de)
        {
            try
            {
                de.Entry(_ChatMessage).State = System.Data.Entity.EntityState.Modified;
                de.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MarkUnreadChatMessage(int _id, DatabaseEntities de)
        {
            try
            {
                de.ChatMessages.Where(x => x.ChatRoomId == _id && x.IsRead == 0).ToList().ForEach(x => x.IsRead = 1);
                de.SaveChanges();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteChatMessage(int _Id, DatabaseEntities de)
        {
            try
            {
                de.ChatMessages.Remove(de.ChatMessages.Where(x => x.Id == _Id).FirstOrDefault());
                de.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ClearUnreadMessage(int _Id, int _userId, DatabaseEntities de)
        {
            try
            {
                de.ChatMessages.Where(x => x.ChatRoomId == _Id && x.SenderId != _userId && x.IsRead == 0).ToList().ForEach(x => x.IsRead = 1);
                de.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}