
using AdmailLdapService.BL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdmailLdapService
{
    public class LdapServiceMain
    {

        private readonly LdapService _ldapService;
        private readonly LdapServiceNovell _ldapServiceNovell;
        private readonly LdapServiceLinqToLdap _ldapServiceLinqToLdap;
        ILogger<LdapServiceLinqToLdap> _logger;

        public LdapServiceMain(LdapService ldapService, LdapServiceNovell ldapServiceNovell, LdapServiceLinqToLdap ldapServiceLinqToLdap, ILogger<LdapServiceLinqToLdap> logger)
        {
            _ldapService = ldapService;
            _ldapServiceNovell = ldapServiceNovell;
            _ldapServiceLinqToLdap = ldapServiceLinqToLdap;
            _logger = logger;
        }

        public void Run()
        {

            
            _ldapService.LoadLdapUsers();
            _ldapServiceNovell.LoadLdapUsersAsync();
            _ldapServiceLinqToLdap.LoadLdapUsers();
        }
    }
}
