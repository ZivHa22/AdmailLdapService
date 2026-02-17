
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdmailLdapService.Models
{
    [Table("usersgroups")]
    public partial class Usersgroup
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [StringLength(100)]
        public string UserName { get; set; } = null!;

        [StringLength(100)]
        public string GroupName { get; set; } = null!;


        public Usersgroup(string UserName, string GroupName)
        {
            this.UserName = UserName;
            this.GroupName = GroupName;
        }
    }

}
