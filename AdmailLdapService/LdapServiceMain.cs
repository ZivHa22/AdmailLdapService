
using AdmailLdapService.BL;
using Microsoft.Extensions.Logging;

namespace AdmailLdapService
{
    public class LdapServiceMain
    {

        private readonly LdapService _ldapService;
        private readonly LdapServiceNovell _ldapServiceNovell;
        private readonly LdapServiceDirectoryWrapper _ldapServiceDirectoryWrapper;
        ILogger<LdapServiceLinqToLdap> _logger;

        public LdapServiceMain(LdapService ldapService, LdapServiceNovell ldapServiceNovell, LdapServiceDirectoryWrapper ldapServiceDirectoryWrapper, ILogger<LdapServiceLinqToLdap> logger)
        {
            _ldapService = ldapService;
            _ldapServiceNovell = ldapServiceNovell;
            _ldapServiceDirectoryWrapper = ldapServiceDirectoryWrapper;
            _logger = logger;
        }

        public void Run()
        {

            _logger.LogInformation($"Start _ldapService DirectoryServices.Protocols");
            _ldapService.LoadLdapUsers();
            _logger.LogInformation($"End _ldapService DirectoryServices.Protocols");

            _logger.LogInformation($"Start _ldapService ldapServiceNovell");
            _ldapServiceNovell.LoadLdapUsersAsync();
            _logger.LogInformation($"End _ldapService ldapServiceNovell");

            //_logger.LogInformation($"Start _ldapService LoadLdapUsersAndGroupsAsync");
            //_ldapServiceDirectoryWrapper.LoadLdapUsersAndGroupsAsync();
            //_logger.LogInformation($"End _ldapService DirectoryServices.Protocols");
        }
    }
}
