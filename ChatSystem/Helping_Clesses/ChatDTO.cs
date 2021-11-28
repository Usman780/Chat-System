using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatSystem.Helping_Clesses
{
    public class ChatDTO
    {
        public int Id {set; get;}
        public string AgentName {set; get;}
        public string StartedAt {set; get;}
        public string ClosedAt {set; get;}
        public int TotalMessages {set; get;}
    }
}