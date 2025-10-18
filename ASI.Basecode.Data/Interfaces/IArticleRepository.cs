using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IArticleRepository
    {
        IEnumerable<Article> GetAll();
        Article GetById(int id);
        void Add(Article article);
        void Update(Article article);
        void Delete(int id);
    }
}
