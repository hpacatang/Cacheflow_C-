using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services
{
    public class ArticleService
    {
        private readonly IArticleRepository _repository;

        public ArticleService(IArticleRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Article> GetAllArticles()
        {
            return _repository.GetAll();
        }

        public Article GetArticleById(int id)
        {
            return _repository.GetById(id);
        }

        public void AddArticle(Article article)
        {
            article.LastUpdated = DateTime.UtcNow;
            _repository.Add(article);
        }

        public void UpdateArticle(Article article)
        {
            article.LastUpdated = DateTime.UtcNow;
            _repository.Update(article);
        }

        public void DeleteArticle(int id)
        {
            _repository.Delete(id);
        }
    }
}
