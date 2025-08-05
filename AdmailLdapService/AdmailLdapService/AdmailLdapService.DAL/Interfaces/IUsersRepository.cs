using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdmailLdapService.Models;

namespace AdmailLdapService.DAL.Interfaces
{
    public interface IUsersRepository
    {

        public void AddUsersAd(List<Domainuser> users);
        public void AddUserAd(Domainuser user);
        public void AddGroups(List<Domainuser> groups);

        public void DeleteAllDomainUsers();
    }
}
