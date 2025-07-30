using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.Models;

namespace AdmailLdapService.BL
{
    public class LdapService
    {


        private readonly ITblAdministrationRepository tblAdministrationRepository;
        public LdapService(ITblAdministrationRepository _tblAdministrationRepositor)
        {
            tblAdministrationRepository = _tblAdministrationRepositor;
        }

        public void LoadLdapUsers()
        {
            LdapDetail ldapDetail = tblAdministrationRepository.GetldapDetails();
            var identifier = new LdapDirectoryIdentifier(ldapDetail.Host, ldapDetail.Port);
            var credentials = new NetworkCredential(ldapDetail.BindDn, ldapDetail.Password);
            using var connection = new LdapConnection(identifier, credentials)
            {
                AuthType = AuthType.Basic
            };

            connection.Bind();

            Console.WriteLine("Connected to LDAP\n");

            // Search for users
            string userFilter = "(objectClass=user)";
            var userRequest = new SearchRequest(ldapDetail.BaseDn, userFilter, SearchScope.Subtree, new[] { "cn" });
            var userResponse = (SearchResponse)connection.SendRequest(userRequest);

            Console.WriteLine("=== Users ===");
            foreach (SearchResultEntry entry in userResponse.Entries)
            {
                Console.WriteLine("User CN: " + entry.Attributes["cn"]?[0]);
            }
        }
    }
}
