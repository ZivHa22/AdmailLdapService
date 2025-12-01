using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdmailLdapService.Models;
using LinqToLdap.Mapping;
using LinqToLdap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdmailLdapService.BL
{
    public class LdapServiceLinqToLdap
    {
        private readonly SecurityService securityService;
        private readonly ILogger<LdapServiceLinqToLdap> logger;
        private readonly IConfiguration configuration;

        public LdapServiceLinqToLdap(
            ILogger<LdapServiceLinqToLdap> _logger,
            SecurityService _securityService,
            IConfiguration _configuration)
        {
            logger = _logger;
            securityService = _securityService;
            configuration = _configuration;
        }

        public void LoadLdapUsers()
        {
            // 1. קריאת פרטי LDAP מה-configuration (כמו אצלך)
            var ldapDetail = new LdapDetail
            {
                Id = 1,
                BaseDn = configuration["LdapDetails:baseDn"],
                BindDn = configuration["LdapDetails:bindDn"],
                Host = configuration["LdapDetails:host"],
                Password = configuration["LdapDetails:password"],
                Port = int.Parse(configuration["LdapDetails:port"] ?? "389")
            };

            ldapDetail.Password = securityService.DecryptString(ldapDetail.Password);

            logger.LogInformation(
                "Try to connect to LDAP (LinqToLdap) host:{Host}, port:{Port}, BindDn:{BindDn}, BaseDn:{BaseDn}",
                ldapDetail.Host,
                ldapDetail.Port,
                ldapDetail.BindDn,
                ldapDetail.BaseDn);

            // 2. הגדרת LdapConfiguration
            var config = new LdapConfiguration()
                .MaxPageSizeIs(1000); // כמו בדוגמה הרשמית :contentReference[oaicite:1]{index=1}

            // מיפוי ע"י Attributes (DirectorySchema/DirectoryAttribute)
            config.AddMapping(new AttributeClassMap<LdapUser>());
            config.AddMapping(new AttributeClassMap<LdapGroup>());

            // הגדרת ה-Connection לשרת
            config.ConfigureFactory(ldapDetail.Host)
                  .UsePort(ldapDetail.Port)
                  .AuthenticateBy(System.DirectoryServices.Protocols.AuthType.Basic)
                  .AuthenticateAs(new NetworkCredential(ldapDetail.BindDn, ldapDetail.Password))
                  .ProtocolVersion(3);

            // אפשר: config.UseStaticStorage(); אם אתה רוצה לשתף את ה-Config סטטית

            try
            {
                using var context = new DirectoryContext(config);

                // --------------------
                //     USERS
                // --------------------
                // בגלל ה-DirectorySchema(ObjectClass="user")
                // LinqToLdap כבר מוסיף Filter של objectClass=user
                var ldapUsers = context.Query<LdapUser>().ToList();

                int usersCount = 0;
                foreach (var u in ldapUsers)
                {
                    // בניית adfields כמו בקוד הישן
                    string adfields = "Usergroups=;";
                    adfields += $"name={u.Name};";
                    adfields += $"mail={u.Mail};";

                    var domainuser = new Domainuser(
                        u.Name,
                        false,
                        u.Mail,
                        CleanString(adfields));

                    logger.LogInformation("User:{UserName}. mail:{Mail}",
                        domainuser.UserName,
                        domainuser.UserEmail);

                    usersCount++;
                }

                logger.LogInformation("User import (LinqToLdap) finished. Total users: {Count}", usersCount);

                // --------------------
                //     GROUPS
                // --------------------
                var ldapGroups = context.Query<LdapGroup>().ToList();

                int groupsCount = 0;
                foreach (var g in ldapGroups)
                {
                    logger.LogInformation("Group: {GroupName}", g.Name);

                    var groupUser = new Domainuser(g.Name, true, "", "");
                    logger.LogInformation("groupUser:{GroupUser}", groupUser.UserName);

                    groupsCount++;
                }

                logger.LogInformation(
                    "Group import (LinqToLdap) finished. Total groups: {Count}",
                    groupsCount);
            }
                catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unexpected error while loading LDAP users/groups with LinqToLdap. Host:{Host}, Port:{Port}, BindDn:{BindDn}, BaseDn:{BaseDn}, Message:{Message}",
                    ldapDetail.Host,
                    ldapDetail.Port,
                    ldapDetail.BindDn,
                    ldapDetail.BaseDn,
                    ex.Message);

                throw;
            }
        }

        private string CleanString(string input)
        {
            // כמו אצלך – מנקה תווים לא מודפסים
            return Regex.Replace(input, @"[\x00-\x1F\x7F]", string.Empty);
        }
    }
}
