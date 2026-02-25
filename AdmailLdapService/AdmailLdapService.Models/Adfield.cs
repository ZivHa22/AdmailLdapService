using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


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
