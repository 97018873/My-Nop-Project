using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Stores;
using Nop.Services.Events;

namespace Nop.Services.News
{

    public partial class NewsCategoryService:INewsCategoryService
    {
        private readonly IRepository<NewsCategory> _NewsCategoryRepository;

        public NewsCategoryService(IRepository<NewsCategory> _NewsCategoryRepository) {
            this._NewsCategoryRepository = _NewsCategoryRepository;
        
        }
        public IList<NewsCategory> GetAllNewsCategories()
        {

            var query=_NewsCategoryRepository.Table;
            return query.Where(w => w.Status != 0).ToList<NewsCategory>();
        }

        public NewsCategory GetById(int id) {
            return _NewsCategoryRepository.GetById(id);
        }
    }
}
