using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmailLdapService.Models
{
    public enum UserStatuesError
    {
        EmailExist = 1,
        UserNameExsit = 2,
        GroupExist = 3,
        UserDuplicate = 4,
        NoError = 5
    };
}
