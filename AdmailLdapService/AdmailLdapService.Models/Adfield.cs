using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmailLdapService.Models
{

    [Table("adfields")]
    public partial class Adfield
    {
        [Key]
        [Column("ADFieldName")]
        [StringLength(150)]
        public string AdfieldName { get; set; } = null!;

        public Adfield() { }

        public Adfield(string adfieldName)
        {
            AdfieldName = adfieldName;
        }
    }
}
