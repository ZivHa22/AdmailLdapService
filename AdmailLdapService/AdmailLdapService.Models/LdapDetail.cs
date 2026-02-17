

namespace AdmailLdapService.Models
{
    public partial class LdapDetail
    {
        public int Id { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string BindDn { get; set; }

        public string Password { get; set; }

        public string BaseDn { get; set; }
    }
}
