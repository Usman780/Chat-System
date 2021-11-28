using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.DAL;
using ChatSystem.Models;

namespace ChatSystem.BL
{
    public class ChatRoomBL
    {
        public List<ChatRoom> GetAllChatRoomList(DatabaseEntities de)
        {
            return new ChatRoomDAL().GetAllChatRoomList(de);
        }

        public List<ChatRoom> GetActiveChatRoomList(DatabaseEntities de)
        {
            return new ChatRoomDAL().GetActiveChatRoomList(de);
        }

        public ChatRoom GetChatRoomById(int _Id, DatabaseEntities de)
        {
            return new ChatRoomDAL().GetChatRoomById(_Id, de);
        }

        public ChatRoom GetChatRoomByClientId(int _Id, DatabaseEntities de)
        {
            return new ChatRoomDAL().GetChatRoomByClientId(_Id, de);
        }

        public ChatRoom GetChatRoomByAgenttId(int _Id, DatabaseEntities de)
        {
            return new ChatRoomDAL().GetChatRoomByAgenttId(_Id, de);
        }

        public int AddChatRoom(ChatRoom _ChatRoom, DatabaseEntities de)
        {
            
            return new ChatRoomDAL().AddChatRoom(_ChatRoom, de);
        }

        public bool UpdateChatRoom(ChatRoom _ChatRoom, DatabaseEntities de)
        {
            
            return new ChatRoomDAL().UpdateChatRoom(_ChatRoom, de);
        }

        public bool DeleteChatRoom(int _Id, DatabaseEntities de)
        {
            return new ChatRoomDAL().DeleteChatRoom(_Id, de);
        }

    }
}