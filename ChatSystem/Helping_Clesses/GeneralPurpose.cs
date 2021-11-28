using ChatSystem.BL;
using ChatSystem.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace ChatSystem.Helping_Clesses
{
    public class GeneralPurpose
    {
        private readonly DatabaseEntities de = new DatabaseEntities();

        public User ValidateLoggedinUser()
        {
            //Get the current claims principal
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal; // Get the claims values
            var userId = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            User loggedInUser = new UserBL().GetUserById(Convert.ToInt32(userId), de);

            return loggedInUser;
        }


        public bool ValidateEmailWithCompany(string email = "", string _companyName="", int id = -1)
        {
            int emailCount = 0;

            if (id != -1)
            {
                emailCount = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == _companyName.Trim().ToLower() && x.Email.ToLower() == email.ToLower() && x.Id != id).Count();
            }
            else
            {
                emailCount = new UserBL().GetActiveUserList(de).Where(x => x.Company.Name.ToLower() == _companyName.Trim().ToLower() && x.Email.ToLower() == email.ToLower()).Count();
            }

            if (emailCount > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public bool ValidateEmail(string email = "", int id = -1)
        {
            int emailCount = 0;

            if (id != -1)
            {
                emailCount = new UserBL().GetActiveUserList(de).Where(x => x.Email.ToLower() == email.ToLower() && x.Id != id).Count();
            }
            else
            {
                emailCount = new UserBL().GetActiveUserList(de).Where(x => x.Email.ToLower() == email.ToLower()).Count();
            }

            if (emailCount > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}