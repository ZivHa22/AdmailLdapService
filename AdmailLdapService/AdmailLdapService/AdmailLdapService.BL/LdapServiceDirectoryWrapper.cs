using AdmailLdapService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmailLdapService.BL
{
    public class LdapServiceDirectoryWrapper
    {
        private readonly SecurityService _securityService;
        private readonly ILogger<LdapServiceDirectoryWrapper> _logger;
        private readonly IConfiguration _configuration;
        private readonly DirectoryServicesWrapper _wrapper;

        public LdapServiceDirectoryWrapper(
            ILogger<LdapServiceDirectoryWrapper> logger,
            SecurityService securityService,
            IConfiguration configuration,
            ILogger<DirectoryServicesWrapper> wrapperLogger)
        {
            _logger = logger;
            _securityService = securityService;
            _configuration = configuration;

            var ldapDetail = new LdapDetail
            {
                Id = 1,
                BaseDn = _configuration["LdapDetails:baseDn"],
                BindDn = _configuration["LdapDetails:bindDn"],
                Host = _configuration["LdapDetails:host"],
                Password = _configuration["LdapDetails:password"],
                Port = int.Parse(_configuration["LdapDetails:port"] ?? "389")
            };

            ldapDetail.Password = _securityService.DecryptString(ldapDetail.Password);

            _wrapper = new DirectoryServicesWrapper(ldapDetail, wrapperLogger);
        }

        public async Task LoadLdapUsersAndGroupsAsync()
        {
            _logger.LogInformation(
                "Starting LDAP import via DirectoryServicesWrapper. Host:{Host}, Port:{Port}, BindDn:{BindDn}, BaseDn:{BaseDn}",
                _configuration["LdapDetails:host"],
                _configuration["LdapDetails:port"],
                _configuration["LdapDetails:bindDn"],
                _configuration["LdapDetails:baseDn"]);

            try
            {
                var users =  _wrapper.GetUsersAsync();
                var groups = await _wrapper.GetGroupsAsync();

                _logger.LogInformation("LDAP import finished. Users:{UsersCount}, Groups:{GroupsCount}",
                    users.Count,
                    groups.Count);

                // כאן אתה יכול:
                // - לשמור ל-DB
                // - להתאים לטבלאות שלך
                // - להשוות למשתמשים קיימים וכו'
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error in LoadLdapUsersAndGroupsAsync. Message:{Message}",
                    ex.Message);
                throw;
            }
        }
    }
}
