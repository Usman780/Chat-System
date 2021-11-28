using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatSystem.Models;

namespace ChatSystem.DAL
{
    public class CompanyDAL
    {

        public List<Company> GetAllCompanyList(DatabaseEntities de)
        {
            return de.Companies.ToList();
        }

        public List<Company> GetActiveCompanyList(DatabaseEntities de)
        {
            return de.Companies.Where(x => x.IsActive == 1).ToList();
        }

        
        public Company GetCompanyById(int _Id, DatabaseEntities de)
        {
            return de.Companies.Where(x => x.Id == _Id).FirstOrDefault(x => x.IsActive == 1);
        }
        
        public Company GetCompanyByName(string _name, DatabaseEntities de)
        {
            return de.Companies.Where(x => x.Name.ToLower() == _name.ToLower()).FirstOrDefault(x => x.IsActive == 1);
        }

        public int AddCompany(Company _Company, DatabaseEntities de)
        {
            try
            {
                de.Companies.Add(_Company);
                de.SaveChanges();

                return _Company.Id;
            }
            catch
            {
                return -1;
            }
        }

        public bool UpdateCompany(Company _Company, DatabaseEntities de)
        {
            try
            {
                de.Entry(_Company).State = System.Data.Entity.EntityState.Modified;
                de.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCompany(int _Id, DatabaseEntities de)
        {
            try
            {
                de.Companies.Remove(de.Companies.Where(x => x.Id == _Id).FirstOrDefault());
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