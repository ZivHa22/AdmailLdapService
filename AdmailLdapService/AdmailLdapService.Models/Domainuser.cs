using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmailLdapService.Models
{
    public partial class Domainuser
    {
        public int UserId { get; set; }


        public string? UserName { get; set; } = null!;

        public bool IsGroup { get; set; }

  
        public string? UserEmail { get; set; } = null!;

        public string? Adfields { get; set; }

        public Domainuser(string _userName, bool _isGroup, string _userEmail, string _adFields)
        {
            this.UserName = _userName;
            this.IsGroup = _isGroup;
            this.UserEmail = _userEmail;
            this.Adfields = _adFields;
        }
        public Domainuser() { }
    }
}
