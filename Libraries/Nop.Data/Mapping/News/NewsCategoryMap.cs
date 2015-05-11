using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    public class NewsCategoryMap: EntityTypeConfiguration<NewsCategory>
    {
        public NewsCategoryMap() {
            this.ToTable("NewsCategory");
            this.HasKey(pr => pr.Id);
        }
    }
}
