using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Stores;
namespace Nop.Data.Mapping.Stores
{
    public partial class StoreMap : EntityTypeConfiguration<Store>
    {
        public StoreMap()
        {
            this.ToTable("Store");
            this.HasKey(s => s.Id);
            this.Property(s => s.Name).IsRequired().HasMaxLength(400);
            this.Property(s => s.Url).IsRequired().HasMaxLength(400);
            this.Property(s => s.SecureUrl).HasMaxLength(400);
            this.Property(s => s.Hosts).HasMaxLength(1000);
            this.Property(s => s.AppVersion).HasMaxLength(100);
            this.Property(s => s.Details).HasMaxLength(400);
            this.Property(s => s.UpdateUrl).HasMaxLength(400);
        }
    }
}