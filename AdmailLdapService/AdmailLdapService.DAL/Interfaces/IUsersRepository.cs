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

        public UserUpdateStatus AddUserAd(Domainuser user);
        public void DeleteAllDomainUsers();
        public bool InsertUserGroup(List<Usersgroup> usersGroup, string UserName);
        public UserUpdateStatus AddGroup(Domainuser group);
        public void DeleteAllUsersGroups();

        public void deleteAdFields();
        public void AddAdFields(Adfield adfield);
        public Adfield GetAdfield(string adfield);

    }
}