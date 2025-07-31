using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdmailLdapService.DAL.DataAccess;
using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.Models;
using Azure.Core;
using Microsoft.Extensions.Configuration;

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
        public void UpdateUserAd(Domainuser user)
        {
            try
            {
                context.Add(user);
                context.SaveChanges();
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

    }
}
