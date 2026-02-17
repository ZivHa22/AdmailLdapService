using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdmailLdapService.DAL.DataAccess;
using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.Models;
using Microsoft.EntityFrameworkCore;

namespace AdmailLdapService.DAL.Respositories
{
    public class TblAdministrationRepository : ITblAdministrationRepository
    {
        AdmailDbContext context { get; }

        public TblAdministrationRepository(AdmailDbContext _context)
        {
            context = _context;
        }


        public Tbladministration GetAdminDataSettings()
        {
            try
            {
                Tbladministration tbladministration = context.Tbladministrations.AsNoTracking().FirstOrDefault();
                return tbladministration;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void UpdateAdminDataSettings(Tbladministration data)
        {
            try
            {
                context.Tbladministrations.Update(data);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public LdapDetail GetldapDetails()
        {
            try
            {
                return context.LdapDetails.AsNoTracking().FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}