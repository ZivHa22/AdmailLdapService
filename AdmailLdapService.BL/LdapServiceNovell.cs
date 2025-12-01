using System.Text.RegularExpressions;
using AdmailLdapService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;

namespace AdmailLdapService.BL
{
    public class LdapServiceNovell
    {

        private readonly SecurityService securityService;
        private ILogger<LdapService> logger;
        private readonly IConfiguration configuration;
        public LdapServiceNovell(ILogger<LdapService> _logger, SecurityService _securityService, IConfiguration _configuration)
        {
            logger = _logger;
            securityService = _securityService;
            configuration = _configuration;
        }

        public async Task LoadLdapUsersAsync()
        {
            LdapDetail ldapDetail = new LdapDetail
            {
                Id = 1,
                BaseDn = configuration["LdapDetails:baseDn"],
                BindDn = configuration["LdapDetails:bindDn"],
                Host = configuration["LdapDetails:host"],
                Password = configuration["LdapDetails:password"],
                Port = int.Parse(configuration["LdapDetails:port"])
            };
            ldapDetail.Password = securityService.DecryptString(ldapDetail.Password);
            logger.LogInformation($"Try to connect to ldap with host:{ldapDetail.Host}, port: {ldapDetail.Port}, BindDn: {ldapDetail.BindDn} , BaseDn: {ldapDetail.BaseDn}");
            var connection = new LdapConnection();
            try
            {


                // 4. Bind
                connection.ConnectAsync(ldapDetail.Host, ldapDetail.Port);
                
                try
                {
                    connection.BindAsync(ldapDetail.BindDn, ldapDetail.Password);
                    
                }
                catch (LdapException ex)
                {
                    logger.LogError(ex,
                        "LDAP bind FAILED  Host:{Host}, Port:{Port}, BindDn:{BindDn}, BaseDn:{BaseDn}, ErrorCode:{ErrorCode}, Message:{Message}",
                        ldapDetail.Host,
                        ldapDetail.Port,
                        ldapDetail.BindDn,
                        ldapDetail.BaseDn,
                        ex.ResultCode,
                        ex.LdapErrorMessage);

                    throw new LdapException(ex.Message);
                }

 
                string userFilter = "(objectClass=user)";

                LdapSearchResults userResults;
                string name = "";
                string mail = "";
                try
                {
                    userResults = (LdapSearchResults)await connection.SearchAsync(
                        ldapDetail.BaseDn,
                        LdapConnection.ScopeSub,
                        userFilter,
                        null,
                        false
                    );
                }

                catch (LdapException ex)
                {
                    logger.LogError(ex,
                        "LDAP user search FAILED. BaseDn:{BaseDn}, Filter:{Filter} ,Message:{Message}",
                        ldapDetail.BaseDn,
                        userFilter,
                        ex.Message);
                    throw new LdapException(ex.Message);
                }
                int usersCount = 0;

                while (await userResults.HasMoreAsync())
                {
                    LdapEntry entry;
                    try
                    {
                        entry = await userResults.NextAsync();
                    }
                    catch (LdapException ex)
                    {

                        logger.LogWarning(ex, $"Error reading a user entry from LDAP (Novell), skipping entry. Messege:{ex.Message}");
                        continue;
                    }

                    string adfields = "Usergroups=;";

                    LdapAttributeSet attributeSet = entry.GetAttributeSet();
                    var ienum = attributeSet.GetEnumerator();

                    while (ienum.MoveNext())
                    {
                        var attribute = (LdapAttribute)ienum.Current;
                        string attributeName = attribute.Name;

                        string[] values = attribute.StringValueArray;

                        if (values != null)
                        {
                            foreach (var val in values)
                            {
                                if (attributeName == "name")
                                {
                                    name = val;
                                }
                                if (attributeName == "mail")
                                {
                                    mail = val;
                                }
                                adfields += $"{attributeName}={val};";
                            }
                        }
                    }

                    var domainuser = new Domainuser(name, false, mail, CleanString(adfields));
                    logger.LogInformation($"User:{domainuser.UserName}. mail :{ domainuser.UserEmail}" );
                    usersCount++;
                }


                logger.LogInformation("User import (Novell) finished. Total users: {Count}", usersCount);

                // --------------------
                //     GROUPS
                // --------------------
                string groupFilter = "(objectClass=group)";

                LdapSearchResults groupResults;
                try
                {
                    groupResults = (LdapSearchResults)await connection.SearchAsync(
                        ldapDetail.BaseDn,
                        LdapConnection.ScopeSub,
                        groupFilter,
                        new[] { "cn" },   // רק CN
                        false
                    );
                }
                catch (LdapException ex)
                {
                    logger.LogError(ex,
                        "LDAP group search FAILED . BaseDn:{BaseDn}, Filter:{Filter}, Messege: {Messege}",
                        ldapDetail.BaseDn,
                        groupFilter,
                        ex.Message);
                    throw new LdapException(ex.Message);
                }

                int groupsCount = 0;
                while (await groupResults.HasMoreAsync())
                {
                    LdapEntry entry;
                    try
                    {
                        entry = await groupResults.NextAsync();
                    }
                    catch (LdapException ex)
                    {
                        logger.LogWarning(ex, "Error reading a group entry from LDAP (Novell), skipping entry.");
                        continue;
                    }
                    var attributeSet = entry.GetAttributeSet();
                    var cnAttr = attributeSet.GetAttribute("cn");
                    string[] values = cnAttr.StringValueArray;
                    if (cnAttr != null)
                    {
                        string groupName = "";
                        foreach (var val in values)
                        {
                            groupName = val;
                        }

                        logger.LogInformation("Group: {GroupName}", groupName);

                        var groupUser = new Domainuser(groupName, true, "", "");
                        logger.LogInformation($"groupUser:{groupUser.UserName}");
                        groupsCount++;
                    }
                }
            }
            catch (LdapException ex)
            {
                logger.LogError(ex,
                    ex.Message,
                    ldapDetail.Host,
                    ldapDetail.Port,
                    ldapDetail.BindDn,
                    ldapDetail.BaseDn);
                throw new LdapException(ex.Message); ;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unexpected error while loading LDAP users (Novell). Host:{Host}, Port:{Port}, BindDn:{BindDn}, BaseDn:{BaseDn}, {Messege}",
                    ldapDetail.Host,
                    ldapDetail.Port,
                    ldapDetail.BindDn,
                    ldapDetail.BaseDn,
                    ex.Message);
                throw new LdapException(ex.Message); ;
            }
            finally
            {
                connection.Disconnect();
            }

        }
        private string CleanString(string input)
        {
            // Removes all control characters (ASCII codes < 32 except newline, tab, etc.)
            return Regex.Replace(input, @"[\x00-\x1F\x7F]", string.Empty);
        }
    }
}
