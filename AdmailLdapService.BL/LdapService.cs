    using AdmailLdapService.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.DirectoryServices.Protocols;

    using System.Net;
    using System.Text.RegularExpressions;


    namespace AdmailLdapService.BL
    {
        public class LdapService
        {



            private readonly SecurityService securityService;
            private ILogger<LdapService> logger;
            private readonly IConfiguration configuration;
            public LdapService(ILogger<LdapService> _logger,SecurityService _securityService,IConfiguration _configuration)
            {
                logger = _logger;   
                securityService = _securityService;
                configuration = _configuration;
            }

            public void LoadLdapUsers()
            {
                LdapDetail ldapDetail = new LdapDetail
                {
                    Id =1,
                    BaseDn = configuration["LdapDetails:baseDn"],
                    BindDn = configuration["LdapDetails:bindDn"],
                    Host = configuration["LdapDetails:host"],
                    Password = configuration["LdapDetails:password"],
                    Port = int.Parse(configuration["LdapDetails:port"])
                };

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
                    // 3. Get all users (objectClass=user)

                    string ldapFilter = "(objectClass=user)";

                    var request = new SearchRequest(
                        ldapDetail.BaseDn,
                        ldapFilter,
                        SearchScope.Subtree,
                        null
                    );
                    var response = (SearchResponse)connection.SendRequest(request);

                    logger.LogInformation("Users found:");

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
                                    logger.LogInformation($"{attrName}: {val}");
                                }
                            }
                        }

                        string cn = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A";
                        string mail = entry.Attributes["mail"]?[0]?.ToString() ?? "N/A";
                   

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
                            logger.LogInformation($"{entry.Attributes["cn"][0]}");
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.LogError($"LDAP Error: {ex.Message}");
                }
            }
        }
    }
