using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.News;

namespace Nop.Services.News
{
    public interface INewsCategoryService
    {
        IList<NewsCategory> GetAllNewsCategories();
        NewsCategory GetById(int id);

    }
}
