using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AsiBasecodeDBContext _context;

        public ArticleRepository(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        // get all articles
        public IEnumerable<Article> GetAll()
        {
            return _context.Articles.ToList();
        }

        // get a single article by ID
        public Article GetById(int id)
        {
            return _context.Articles.Find(id);
        }

        // add a new article
        public void Add(Article article)
        {
            _context.Articles.Add(article);
            _context.SaveChanges();
        }

        // update an existing article
        public void Update(Article article)
        {
            _context.Articles.Update(article);
            _context.SaveChanges();
        }

        // delete an article by ID
        public void Delete(int id)
        {
            var article = _context.Articles.Find(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                _context.SaveChanges();
            }
        }
    }
}
