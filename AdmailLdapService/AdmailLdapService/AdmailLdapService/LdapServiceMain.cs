using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdmailLdapService.BL;

namespace AdmailLdapService
{
    public class LdapServiceMain
    {

        private readonly LdapService _ldapService;  
        public LdapServiceMain(LdapService ldapService)
        {
            _ldapService = ldapService;
        }

        public void Run()
        {
            _ldapService.LoadLdapUsers();
        }
    }
}
