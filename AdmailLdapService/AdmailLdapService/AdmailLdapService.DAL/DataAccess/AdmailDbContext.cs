using AdmailLdapService.Models;
using Microsoft.EntityFrameworkCore;

namespace AdmailLdapService.DAL.DataAccess
{
    public partial class AdmailDbContext : DbContext
    {
        public AdmailDbContext()
        {
        }

        public AdmailDbContext(DbContextOptions<AdmailDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Tbladministration> Tbladministrations { get; set; }
        public virtual DbSet<Domainuser> Domainusers { get; set; }
        public virtual DbSet<LdapDetail> LdapDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-DV8BF2T\\SQLEXPRESS;Database=AdmailAzureUsers;Integrated Security=True;TrustServerCertificate=True");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tbladministration>(entity =>
            {
                entity.HasKey(e => e.CustomerId);

                entity.ToTable("tbladministration");

                entity.Property(e => e.CustomerId)
                    .ValueGeneratedNever()
                    .HasColumnName("CustomerID");
                entity.Property(e => e.AdminEmail)
                    .HasMaxLength(300)
                    .IsUnicode(false);
                entity.Property(e => e.BannerByDomain).HasColumnName("bannerByDomain");
                entity.Property(e => e.BannerUrl)
                    .HasMaxLength(300)
                    .IsUnicode(false);
                entity.Property(e => e.DefaultBanner).HasColumnName("defaultBanner");
                entity.Property(e => e.DefaultBanner1)
                    .HasMaxLength(300)
                    .IsUnicode(false);
                entity.Property(e => e.DefaultBanner2)
                    .HasMaxLength(300)
                    .IsUnicode(false);
                entity.Property(e => e.DefaultBannerLink)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("defaultBannerLink");
                entity.Property(e => e.DefaultBannerPosition).HasColumnName("defaultBannerPosition");
                entity.Property(e => e.DomainUserName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.DomainUserPassword)
                    .HasMaxLength(300)
                    .IsUnicode(false);
                entity.Property(e => e.Embeded).HasDefaultValue(0);
                entity.Property(e => e.FromMail)
                    .HasMaxLength(300)
                    .IsUnicode(false);
                entity.Property(e => e.LastLoad)
                    .HasColumnType("datetime")
                    .HasColumnName("lastLoad");
                entity.Property(e => e.Ldap)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("LDAP");
                entity.Property(e => e.LinkType)
                    .HasMaxLength(45)
                    .IsUnicode(false)
                    .HasColumnName("linkType");
                entity.Property(e => e.NumOfAuthorizedSigs)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.WhiteBannerSet).HasColumnName("whiteBannerSet");
                entity.Property(e => e.WhiteBannerUrl)
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnName("whiteBannerURL");
                entity.Property(e => e.WidgetMaxSize).HasColumnName("widgetMaxSize");
            });
            modelBuilder.Entity<LdapDetail>(entity =>
            {
                entity.Property(e => e.BaseDn).HasMaxLength(50);
                entity.Property(e => e.BindDn).HasMaxLength(50);
                entity.Property(e => e.Host).HasMaxLength(50);
                entity.Property(e => e.Password).HasMaxLength(50);
                entity.Property(e => e.Port)
                    .HasMaxLength(50)
                    .HasColumnName("port");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}
