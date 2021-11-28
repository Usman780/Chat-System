using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;
using ChatSystem.DAL;

namespace ChatSystem.BL
{
    public class CompanyBL
    {

        public List<Company> GetAllCompanyList(DatabaseEntities de)
        {
            return new CompanyDAL().GetAllCompanyList(de);
        }

        public List<Company> GetActiveCompanyList(DatabaseEntities de)
        {
            return new CompanyDAL().GetActiveCompanyList(de);
        }

        public Company GetCompanyById(int _Id, DatabaseEntities de)
        {
            return new CompanyDAL().GetCompanyById(_Id, de);
        }

        public Company GetCompanyByName(string _name, DatabaseEntities de)
        {
            return new CompanyDAL().GetCompanyByName(_name, de);
        }

        public int AddCompany(Company _Company, DatabaseEntities de)
        {
            if (_Company.Name == "" || _Company.Email == "" || _Company.Address == "" || _Company.Contact == "" || _Company.Name == null || _Company.Email == null || _Company.Address == null || _Company.Contact == null)
            {
                return -1;
            }
            else
            {
                return new CompanyDAL().AddCompany(_Company, de);
            }
        }

        public bool UpdateCompany(Company _Company, DatabaseEntities de)
        {
            if (_Company.Name == "" || _Company.Email == "" || _Company.Address == "" || _Company.Contact == "" || _Company.Name == null || _Company.Email == null || _Company.Address == null || _Company.Contact == null)
            {
                return false;
            }
            else
            {
                return new CompanyDAL().UpdateCompany(_Company, de);
            }
        }

        public bool DeleteCompany(int _Id, DatabaseEntities de)
        {
            return new CompanyDAL().DeleteCompany(_Id, de);
        }

    }
}