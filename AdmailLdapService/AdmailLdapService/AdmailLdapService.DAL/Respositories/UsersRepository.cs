using AdmailLdapService.DAL.DataAccess;
using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.Models;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmailLdapService.DAL.Respositories
{
    public class UsersRepository :IUsersRepository
    {


        AdmailDbContext context { get; }
        public UsersRepository(AdmailDbContext _context)
        {
            context = _context;
        }
        public void AddUsersAd(List<Domainuser> users)
        {
            try
            {
                context.AddRange(users);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public void AddUserAd(Domainuser user)
        {
            try
            {
                DateTime savedate = DateTime.Now;
                context.Add(user);
                context.SaveChanges();
                UpdateLastLoadUser(savedate);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void AddGroups(List<Domainuser> groups)
        {
            try
            {
                
                context.Domainusers.AddRange(groups);
                context.SaveChanges();
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteAllDomainUsers()
        {
            try
            {
                context.Domainusers.RemoveRange(context.Domainusers.ToList());
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private void UpdateLastLoadUser(DateTime lastLoad)
        {
            try
            {
                Tbladministration tbladministration = context.Tbladministrations.AsNoTracking().FirstOrDefault();
                tbladministration.LastLoad = lastLoad;
                context.ChangeTracker.Clear();
                context.Tbladministrations.Update(tbladministration);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
