using LinqToLdap.Mapping;

namespace AdmailLdapService.Models
{
    [DirectorySchema(NamingContext, ObjectClass = "group")]
    public class LdapGroup
    {
        public const string NamingContext = "DC=yourdomain,DC=local";

        [DistinguishedName]
        public string DistinguishedName { get; set; }

        [DirectoryAttribute("cn")]
        public string Name { get; set; }
    }
}