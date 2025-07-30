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
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [StringLength(100)]
        public string? UserName { get; set; } = null!;

        public bool IsGroup { get; set; }

        public string? UserEmail { get; set; } = null!;

        public string? Adfields { get; set; }

        public Domainuser(string _userName, bool _isGroup, string _userEmail, string _adFields)
        {
            UserName = _userName;
            IsGroup = _isGroup;
            UserEmail = _userEmail;
            Adfields = _adFields;
        }
        public Domainuser() { }
    }
}
