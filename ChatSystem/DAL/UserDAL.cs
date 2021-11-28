using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;

namespace ChatSystem.DAL
{
    public class UserDAL
    {

        public List<User> GetAllUserList(DatabaseEntities de)
        {
            return de.Users.ToList();
        }

        public List<User> GetActiveUserList(DatabaseEntities de)
        {
            return de.Users.Where(x => x.IsActive == 1).ToList();
        }

        public List<User> GetUserListByCompanyId(int _Id, DatabaseEntities de)
        {
            return de.Users.Where(x => x.CompanyId == _Id).ToList();
        }

        public List<User> GetActiveUserListByCompanyId(int _Id, DatabaseEntities de)
        {
            return de.Users.Where(x => x.CompanyId==_Id && x.IsActive == 1).ToList();
        }

        public User GetUserById(int _Id, DatabaseEntities de)
        {
            return de.Users.Where(x => x.Id == _Id).FirstOrDefault(x => x.IsActive == 1);
        }

        public bool AddUser(User _user, DatabaseEntities de)
        {
            try
            {
                de.Users.Add(_user);
                de.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public int AddUser2(User _user, DatabaseEntities de)
        {
            try
            {
                de.Users.Add(_user);
                de.SaveChanges();

                return _user.Id;
            }
            catch
            {
                return -1;
            }
        }

        public bool UpdateUser(User _user, DatabaseEntities de)
        {
            try
            {
                de.Entry(_user).State = System.Data.Entity.EntityState.Modified;
                de.SaveChanges();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteUser(int _Id, DatabaseEntities de)
        {
            try
            {
                de.Users.Remove(de.Users.Where(x => x.Id == _Id).FirstOrDefault());
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