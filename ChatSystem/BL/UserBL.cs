using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;
using ChatSystem.DAL;

namespace ChatSystem.BL
{
    public class UserBL
    {

        public List<User> GetAllUserList(DatabaseEntities de)
        {
            return new UserDAL().GetAllUserList(de);
        }

        public List<User> GetActiveUserList(DatabaseEntities de)
        {
            return new UserDAL().GetActiveUserList(de);
        }

        public List<User> GetUserListByCompanyId(int _Id, DatabaseEntities de)
        {
            return new UserDAL().GetUserListByCompanyId(_Id, de);
        }

        public List<User> GetActiveUserListByCompanyId(int _Id, DatabaseEntities de)
        {
            return new UserDAL().GetActiveUserListByCompanyId(_Id, de);
        }

        public User GetUserById(int _Id, DatabaseEntities de)
        {
            return new UserDAL().GetUserById(_Id, de);
        }

        public bool AddUser(User _user, DatabaseEntities de)
        {
            if (_user.Name == "" || _user.Email == "" || _user.Password == "" || _user.Name == null || _user.Email == null || _user.Password == null)
            {
                return false;
            }
            else
            {
                return new UserDAL().AddUser(_user, de);
            }
        }

        public int AddUser2(User _user, DatabaseEntities de)
        {
            if (_user.Name == "" || _user.Email == "" || _user.Password == "" || _user.Name == null || _user.Email == null || _user.Password == null)
            {
                return -1;
            }
            else
            {
                return new UserDAL().AddUser2(_user, de);
            }
        }

        public bool UpdateUser(User _user, DatabaseEntities de)
        {
            if (_user.Name == "" || _user.Email == "" || _user.Password == "" || _user.Name == null || _user.Email == null || _user.Password == null)
            {
                return false;
            }
            else
            {
                return new UserDAL().UpdateUser(_user, de);
            }
        }

        public bool DeleteUser(int _Id, DatabaseEntities de)
        {
            return new UserDAL().DeleteUser(_Id, de);
        }

    }
}