using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.DirectoryServices.Protocols;

using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace AdmailLdapService.BL
{
    public class LdapService
    {
        private readonly ITblAdministrationRepository tblAdministrationRepository;
        private readonly IUsersRepository usersRepository;
        private readonly SecurityService securityService;
        private ILogger<LdapService> logger;
        public LdapService(ITblAdministrationRepository _tblAdministrationRepositor, IUsersRepository _usersRepository, ILogger<LdapService> _logger, SecurityService _securityService)
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
            logger.LogInformation($"Try to connect to ldap with host:{ldapDetail.Host}, port: {ldapDetail.Port}, BindDn: {ldapDetail.BindDn}, BaseDn: {ldapDetail.BaseDn}");
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
                usersRepository.DeleteAllDomainUsers();
                usersRepository.DeleteAllUsersGroups();
                usersRepository.deleteAdFields();


                var groupDnToName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // --------------------
                //  טעינת קבוצות עם עימוד
                // --------------------
                string groupFilter = "(objectClass=group)";

                // גודל עמוד לקבוצות – אפשר להשתמש באותו pageSize כמו למשתמשים, או אחר
                const int groupPageSize = 200;
                var groupPageControl = new PageResultRequestControl(groupPageSize);

                var groupSearchRequest = new SearchRequest(
                    ldapDetail.BaseDn,
                    groupFilter,
                    SearchScope.Subtree,
                    new[] { "cn" }   // נטען רק CN
                );

                // מוסיפים את ה-PageResultRequestControl לבקשה
                groupSearchRequest.Controls.Add(groupPageControl);

                logger.LogInformation("Begin get groups");

                while (true)
                {
                    var groupSearchResponse = (SearchResponse)connection.SendRequest(groupSearchRequest);

                    logger.LogInformation("Group page returned with {Count} groups.", groupSearchResponse.Entries.Count);

                    foreach (SearchResultEntry entry in groupSearchResponse.Entries)
                    {
                        if (entry.Attributes["cn"] == null)
                            continue;

                        string groupName = entry.Attributes["cn"][0].ToString();
                        string groupDn = entry.DistinguishedName;

                        // שמירה בטבלה של קבוצות (כמו שהיה לך)
                        Domainuser domainuser = new Domainuser(groupName, true, "", "");
                        usersRepository.AddGroup(domainuser);

                        // מילוי המילון DN → שם קבוצה
                        groupDnToName[groupDn] = groupName;
                    }

                    // בדיקת עמוד הבא – בדיוק כמו אצל המשתמשים
                    PageResultResponseControl? groupPageResponse =
                        groupSearchResponse.Controls
                            .OfType<PageResultResponseControl>()
                            .FirstOrDefault();

                    if (groupPageResponse == null || groupPageResponse.Cookie == null || groupPageResponse.Cookie.Length == 0)
                    {
                        // אין יותר עמודים
                        break;
                    }

                    // להגדיר Cookie לעמוד הבא
                    groupPageControl.Cookie = groupPageResponse.Cookie;
                }


                // 3. Get all users (objectClass=user)

                string ldapFilter = "(objectClass=user)";

                const int pageSize = 200;
                var pageControl = new PageResultRequestControl(pageSize);

                var searchRequest = new SearchRequest(
                    ldapDetail.BaseDn,
                    ldapFilter,
                    SearchScope.Subtree,
                    null
                );

                searchRequest.Controls.Add(pageControl);
                logger.LogInformation("Begin get users");
                bool firstUser = true;
                while (true)
                {
                    var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

                    logger.LogInformation("Page returned with {Count} users.", searchResponse.Entries.Count);

                    foreach (SearchResultEntry entry in searchResponse.Entries)
                    {
                        if (!IsValidEmail(entry.Attributes["mail"]?[0]?.ToString()))
                        {
                            continue;
                        }
                        string Usergroups = "Usergroups=";


                        var usersGroups = new List<string>();
                        if (entry.Attributes["memberOf"] is { Count: > 0 } memberOfAttr)
                        {


                            foreach (var groupDnObj in memberOfAttr)
                            {
                                if (groupDnObj is byte[] bytes)
                                {
                                    string groupDn = Encoding.UTF8.GetString(bytes);

                                    // מנסים לתרגם DN → שם קבוצה (CN) מהמילון שבנינו קודם
                                    if (groupDnToName.TryGetValue(groupDn, out string groupName))
                                    {
                                        usersGroups.Add(groupName);
                                        Usergroups += groupName + "-";
                                    }
                                    else
                                    {
                                        // אם הקבוצה לא הייתה במילון (לא נטענה מסיבה כלשהי) – אפשר לוג
                                        logger.LogWarning("User {Mail} is member of group DN {GroupDn}, but group not found in dictionary.", entry.Attributes["mail"]?[0]?.ToString(), groupDn);
                                    }
                                }

                            }
                            if (usersGroups.Count > 0)
                                Usergroups = Usergroups.Remove(Usergroups.Length - 1);

                            Usergroups += ";";
                            List<Usersgroup> usersGroupsList = new List<Usersgroup>();

                            foreach (string group in usersGroups)
                            {
                                usersGroupsList.Add(new Usersgroup(entry.Attributes["cn"]?[0]?.ToString(), group));
                            }
                            usersRepository.InsertUserGroup(usersGroupsList, entry.Attributes["cn"]?[0]?.ToString());





                        }
                        string adfields = "";
                 
                        foreach (string attrName in entry.Attributes.AttributeNames)
                        {


                            if (firstUser)
                            {
                                Adfield adfield = new Adfield(attrName);
                                usersRepository.AddAdFields(adfield);
                            }

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
                                        adfields += $"{attrName}={Encoding.UTF8.GetString(bytes)};";
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
                        Domainuser domainuser = new Domainuser(cn, false, mail, CleanString(Usergroups + adfields));
                        usersRepository.AddUserAd(domainuser);
                        firstUser = false;
                        logger.LogInformation($"User: {cn},Email : {mail}");

                    }

                    PageResultResponseControl? pageResponse =
                      searchResponse.Controls
                          .OfType<PageResultResponseControl>()
                          .FirstOrDefault();

                    if (pageResponse == null || pageResponse.Cookie == null || pageResponse.Cookie.Length == 0)
                    {
                        // אין יותר עמודים
                        break;
                    }

                    // להגדיר Cookie לעמוד הבא
                    pageControl.Cookie = pageResponse.Cookie;
                }


            }
            catch (Exception ex)
            {
                logger.LogError($"Field to connect to ldap with host:{ldapDetail.Host}, port: {ldapDetail.Port}, BindDn: {ldapDetail.BindDn}" +
                $", BaseDn: {ldapDetail.BaseDn}");
                logger.LogError(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }


        private string CleanString(string input)
        {
            // Removes all control characters (ASCII codes < 32 except newline, tab, etc.)
            return Regex.Replace(input, @"[\x00-\x1F\x7F]", string.Empty);
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
