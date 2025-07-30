using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmailLdapService.Models
{
    public partial class Tbladministration
    {
        public int CustomerId { get; set; }

        public int Language { get; set; }

        public int? Direction { get; set; }

        public string? BannerUrl { get; set; }

        public int? MaxBannerSize { get; set; }

        public string? AdminEmail { get; set; }

        public string? DomainUserName { get; set; }

        public string? DomainUserPassword { get; set; }

        public string? Ldap { get; set; }

        public int? SenderRule { get; set; }

        public int? ReceiverRule { get; set; }

        public string? FromMail { get; set; }

        public int? SendReports { get; set; }

        public int? DefaultBanner { get; set; }

        public string? DefaultBannerLink { get; set; }

        public int? Embeded { get; set; }

        public int? DefaultBannerPosition { get; set; }

        public string? DefaultBanner1 { get; set; }

        public string? DefaultBanner2 { get; set; }

        public bool? IsWinAuthentication { get; set; }

        public int? CacheRefreshInterval { get; set; }

        public int? CacheRefreshTime { get; set; }

        public string? WhiteBannerUrl { get; set; }

        public int? WhiteBannerSet { get; set; }

        public int? WidgetMaxSize { get; set; }

        public int? IconMaxSize { get; set; }

        public string? LinkType { get; set; }

        public int? NumOfAuthorized { get; set; }

        public string? NumOfAuthorizedSigs { get; set; }

        public int? SenderRuleSig { get; set; }

        public int? ReceiverRuleSig { get; set; }

        public DateTime? LastLoad { get; set; }

        public bool? BannerByDomain { get; set; }

        public bool? TableClient { get; set; }

        public bool? ByFourLetters { get; set; }
    }
}
