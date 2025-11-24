using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToLdap.Mapping;

namespace AdmailLdapService.Models
{
    // כל האובייקטים מסוג user תחת ה-BaseDn שלך
    [DirectorySchema(NamingContext, ObjectCategory = "person", ObjectClass = "user")]
    public class LdapUser
    {
        // תעדכן כאן ל-BaseDn שלך (אותו אחד מה-configuration)
        public const string NamingContext = "DC=yourdomain,DC=local";

        [DistinguishedName]
        public string DistinguishedName { get; set; }

        // name – כמו שהשתמשת ב-Novell
        [DirectoryAttribute("name")]
        public string Name { get; set; }

        // mail – כמו בקוד שלך
        [DirectoryAttribute("mail")]
        public string Mail { get; set; }

        // לא חובה, רק דוגמה לעוד שדות שאפשר להרחיב
        [DirectoryAttribute("sAMAccountName")]
        public string SamAccountName { get; set; }

        [DirectoryAttribute("memberOf")]
        public string[] MemberOf { get; set; }
    }
}
