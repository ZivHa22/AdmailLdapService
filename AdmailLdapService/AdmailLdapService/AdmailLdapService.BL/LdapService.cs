using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.Models;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.DirectoryServices.Protocols;

using System.Net;
using System.Text.RegularExpressions;


namespace AdmailLdapService.BL
{
    public class LdapService
    {


        private readonly ITblAdministrationRepository tblAdministrationRepository;
        private readonly IUsersRepository usersRepository;
        private readonly SecurityService securityService;
        private ILogger<LdapService> logger;
        public LdapService(ITblAdministrationRepository _tblAdministrationRepositor, IUsersRepository _usersRepository,ILogger<LdapService> _logger,SecurityService _securityService)
        {
            tblAdministrationRepository = _tblAdministrationRepositor;
            usersRepository = _usersRepository;
            logger = _logger;   
            securityService = _securityService;
        }

        public void LoadLdapUsers()
        {
            LdapDetail ldapDetail = tblAdministrationRepository.GetldapDetails();
            ldapDetail.Password = securityService.DecryptString(ldapDetail.Password);
            try
            {

                var identifier = new LdapDirectoryIdentifier(ldapDetail.Host, ldapDetail.Port);
                var credentials = new NetworkCredential(ldapDetail.BindDn, ldapDetail.Password);
                using var connection = new LdapConnection(identifier, credentials, AuthType.Basic)
                {
                    SessionOptions =
                    {
                        ProtocolVersion = 3,
                        ReferralChasing = ReferralChasingOptions.None
                    }
                };



                connection.Bind(); // 2. Authenticate
                logger.LogInformation("LDAP bind successful.");
                usersRepository.DeleteAllDomainUsers();
                // 3. Get all users (objectClass=user)

                string ldapFilter = "(objectClass=user)";

                var request = new SearchRequest(
                    ldapDetail.BaseDn,
                    ldapFilter,
                    SearchScope.Subtree,
                    null
                );
                var response = (SearchResponse)connection.SendRequest(request);

                Console.WriteLine("Users found:");

                foreach (SearchResultEntry entry in response.Entries)
                {
                    string adfields = "Usergroups=;";
                    foreach (string attrName in entry.Attributes.AttributeNames)
                    {
                        var values = entry.Attributes[attrName];
                        foreach (var val in values)
                        {
                            if (val is byte[] bytes)
                            {
                                // Handle known binary attributes
                                if (attrName.Equals("cn", StringComparison.OrdinalIgnoreCase) || attrName.Equals("mail", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                else
                                {
                                    // For unknown binary data, convert to Base64
                                    adfields += $"{attrName}={System.Text.Encoding.UTF8.GetString(bytes)};";
                                }
                            }
                            else
                            {
                                Console.WriteLine($"{attrName}: {val}");
                            }
                        }
                    }

                    string cn = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A";
                    string mail = entry.Attributes["mail"]?[0]?.ToString() ?? "N/A";
                    Domainuser domainuser = new Domainuser(cn, false, mail, CleanString(adfields));
                    usersRepository.AddUserAd(domainuser);

                }

                string groupFilter = "(objectClass=group)";
                var requestGroups = new SearchRequest(
                       ldapDetail.BaseDn,  // base DN
                        groupFilter,
                        SearchScope.Subtree,
                         new[] { "cn" }  // get all attributes
                        );
                var responseGroups = (SearchResponse)connection.SendRequest(request);
                foreach (SearchResultEntry entry in response.Entries)
                {
                    if (entry.Attributes["cn"] != null)
                    {
                        Console.WriteLine(entry.Attributes["cn"][0]);
                        Domainuser domainuser = new Domainuser(entry.Attributes["cn"][0].ToString(), true, "", "");
                        usersRepository.AddUserAd(domainuser);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"LDAP Error: {ex.Message}");
            }
        }


        private string CleanString(string input)
        {
            // Removes all control characters (ASCII codes < 32 except newline, tab, etc.)
            return Regex.Replace(input, @"[\x00-\x1F\x7F]", string.Empty);
        }
    }
}
