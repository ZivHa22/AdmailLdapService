
using AdmailLdapService.BL;
using Microsoft.Extensions.Logging;

namespace AdmailLdapService
{
    public class LdapServiceMain
    {

        private readonly LdapService _ldapService;
        ILogger<LdapServiceMain> _logger;

        public LdapServiceMain(LdapService ldapService, ILogger<LdapServiceMain> logger)
        {
            _ldapService = ldapService;

            _logger = logger;
        }

        public void Run()
        {

            _logger.LogInformation($"Start _ldapService DirectoryServices.Protocols");
            _ldapService.LoadLdapUsers();
            _logger.LogInformation($"End _ldapService DirectoryServices.Protocols");
        }
    }
}
