using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdmailLdapService.Models;

namespace AdmailLdapService.DAL.Interfaces
{
    public interface ITblAdministrationRepository
    {
        public Tbladministration GetAdminDataSettings();
        public void UpdateAdminDataSettings(Tbladministration data);
        public LdapDetail GetldapDetails();
    }
}