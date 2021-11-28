using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;

namespace ChatSystem.DAL
{
    public class ChatRoomDAL
    {

        public List<ChatRoom> GetAllChatRoomList(DatabaseEntities de)
        {
            return de.ChatRooms.ToList();
        }

        public List<ChatRoom> GetActiveChatRoomList(DatabaseEntities de)
        {
            return de.ChatRooms.Where(x => x.IsActive == 1).ToList();
        }


        public ChatRoom GetChatRoomById(int _Id, DatabaseEntities de)
        {
            return de.ChatRooms.Where(x => x.Id == _Id).FirstOrDefault();
        }
        
        public ChatRoom GetChatRoomByClientId(int _Id, DatabaseEntities de)
        {
            return de.ChatRooms.Where(x => x.ClientId == _Id).FirstOrDefault(x=> x.IsActive == 1);
        }
        
        public ChatRoom GetChatRoomByAgenttId(int _Id, DatabaseEntities de)
        {
            return de.ChatRooms.Where(x => x.AgentId == _Id).FirstOrDefault(x => x.IsActive == 1);
        }

        public int AddChatRoom(ChatRoom _ChatRoom, DatabaseEntities de)
        {
            try
            {
                de.ChatRooms.Add(_ChatRoom);
                de.SaveChanges();

                return _ChatRoom.Id;
            }
            catch
            {
                return -1;
            }
        }

        public bool UpdateChatRoom(ChatRoom _ChatRoom, DatabaseEntities de)
        {
            try
            {
                de.Entry(_ChatRoom).State = System.Data.Entity.EntityState.Modified;
                de.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteChatRoom(int _Id, DatabaseEntities de)
        {
            try
            {
                de.ChatRooms.Remove(de.ChatRooms.Where(x => x.Id == _Id).FirstOrDefault());
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