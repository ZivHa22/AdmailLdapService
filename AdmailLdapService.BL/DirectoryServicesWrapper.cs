using AdmailLdapService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdmailLdapService.BL
{
    public class DirectoryServicesWrapper
    {
        private readonly LdapDetail _ldapDetail;
        private readonly ILogger<DirectoryServicesWrapper> _logger;

        // ldapPath הכללי – כולל Host + Port + BaseDn
        private string LdapPath =>
            $"LDAP://{_ldapDetail.Host}:{_ldapDetail.Port}/{_ldapDetail.BaseDn}";

        public DirectoryServicesWrapper(
            LdapDetail ldapDetail,
            ILogger<DirectoryServicesWrapper> logger)
        {
            _ldapDetail = ldapDetail;
            _logger = logger;
        }

        private DirectoryEntry CreateDirectoryEntry()
        {
            _logger.LogInformation(
                "Connecting to LDAP via DirectoryServices. Path: {Path}, BindDn: {BindDn}",
                LdapPath,
                _ldapDetail.BindDn);

            // אם אתה ב-AD – בדרך כלל צריך user@domain או DOMAIN\\user
            return new DirectoryEntry(LdapPath, _ldapDetail.BindDn, _ldapDetail.Password);
        }

        private static string CleanString(string input)
        {
            return Regex.Replace(input ?? string.Empty, @"[\x00-\x1F\x7F]", string.Empty);
        }

        /// <summary>
        /// מחזיר את כל המשתמשים (objectClass=user)
        /// </summary>
        public List<Domainuser> GetUsersAsync()
        {
            //return Task.Run(() =>
            //{
                var result = new List<Domainuser>();

                using var entry = CreateDirectoryEntry();
                using var searcher = new DirectorySearcher(entry)
                {
                    Filter = "(objectClass=user)",
                    SearchScope = SearchScope.Subtree
                };

                // אפשר לצמצם כדי לשפר ביצועים
                searcher.PropertiesToLoad.Add("name");
                searcher.PropertiesToLoad.Add("mail");
                searcher.PropertiesToLoad.Add("memberOf");

                SearchResultCollection users;
                try
                {
                    users = searcher.FindAll();
                }
                catch (DirectoryServicesCOMException ex)
                {
                    _logger.LogError(ex,
                        "LDAP user search FAILED (DirectoryServices). BaseDn:{BaseDn}, Filter:{Filter}",
                        _ldapDetail.BaseDn,
                        searcher.Filter);
                    throw;
                }

                _logger.LogInformation("Found {Count} user entries from LDAP.", users.Count);

                foreach (SearchResult sr in users)
                {
                    string name = string.Empty;
                    string mail = string.Empty;
                    string adfields = "Usergroups=;";

                    foreach (string propName in sr.Properties.PropertyNames)
                    {
                        var values = sr.Properties[propName];
                        if (values == null || values.Count == 0) continue;

                        foreach (var val in values)
                        {
                            string strVal = val?.ToString() ?? string.Empty;

                            if (propName == "name")
                                name = strVal;

                            if (propName == "mail")
                                mail = strVal;

                            adfields += $"{propName}={strVal};";
                        }
                    }

                    var domainuser = new Domainuser(name, false, mail, CleanString(adfields));
                    _logger.LogInformation("User: {UserName}, Mail: {Mail}",
                        domainuser.UserName, domainuser.UserEmail);

                    result.Add(domainuser);
                }

                _logger.LogInformation("User import (DirectoryServicesWrapper) finished. Total users: {Count}", result.Count);

                return result;
            //});
        }

        /// <summary>
        /// מחזיר את כל הקבוצות (objectClass=group)
        /// </summary>
        public Task<List<Domainuser>> GetGroupsAsync()
        {
            return Task.Run(() =>
            {
                var result = new List<Domainuser>();

                using var entry = CreateDirectoryEntry();
                using var searcher = new DirectorySearcher(entry)
                {
                    Filter = "(objectClass=group)",
                    SearchScope = SearchScope.Subtree
                };

                searcher.PropertiesToLoad.Add("cn");

                SearchResultCollection groups;
                try
                {
                    groups = searcher.FindAll();
                }
                catch (DirectoryServicesCOMException ex)
                {
                    _logger.LogError(ex,
                        "LDAP group search FAILED (DirectoryServices). BaseDn:{BaseDn}, Filter:{Filter}",
                        _ldapDetail.BaseDn,
                        searcher.Filter);
                    throw;
                }

                _logger.LogInformation("Found {Count} group entries from LDAP.", groups.Count);

                foreach (SearchResult sr in groups)
                {
                    string groupName = string.Empty;

                    if (sr.Properties.Contains("cn") && sr.Properties["cn"].Count > 0)
                    {
                        groupName = sr.Properties["cn"][0]?.ToString() ?? string.Empty;
                    }

                    if (!string.IsNullOrWhiteSpace(groupName))
                    {
                        var groupUser = new Domainuser(groupName, true, "", "");
                        _logger.LogInformation("Group: {GroupName}", groupUser.UserName);
                        result.Add(groupUser);
                    }
                }

                _logger.LogInformation("Group import (DirectoryServicesWrapper) finished. Total groups: {Count}", result.Count);

                return result;
            });
        }
    }
}
