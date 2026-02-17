using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmailLdapService.Models
{
    public class UserUpdateStatus
    {
        public bool seccussUpdate { get; set; }
        public int errorCode { get; set; }
        public string userDuplicate { get; set; }

        public Domainuser domainuserSaved { get; set; }
    }
}
