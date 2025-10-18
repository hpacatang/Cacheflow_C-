using System.Linq;
using System;
using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public ArticleController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        // get all articles
        [HttpGet]
        public IActionResult GetAllArticles()
        {
            var articles = _context.Articles.ToList();
            return Ok(articles);
        }

        // get a specific article
        [HttpGet("{id}")]
        public IActionResult GetArticleById(int id)
        {
            var article = _context.Articles.Find(id);
            if (article == null)
            {
                return NotFound();
            }
            return Ok(article);
        }

        // add a new article
        [HttpPost]
        public IActionResult AddArticle([FromBody] Article article)
        {
            if (ModelState.IsValid)
            {
                article.LastUpdated = DateTime.UtcNow;
                _context.Articles.Add(article);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetArticleById), new { id = article.Id }, article);
            }
            return BadRequest(ModelState);
        }

        // update an article
        [HttpPut("{id}")]
        public IActionResult UpdateArticle(int id, [FromBody] Article article)
        {
            if (id != article.Id)
            {
                return BadRequest();
            }

            var existingArticle = _context.Articles.Find(id);
            if (existingArticle == null)
            {
                return NotFound();
            }

            existingArticle.Title = article.Title;
            existingArticle.Body = article.Body;
            existingArticle.Category = article.Category;
            existingArticle.LastUpdated = DateTime.UtcNow;

            _context.SaveChanges();
            return NoContent();
        }

        // delete an article
        [HttpDelete("{id}")]
        public IActionResult DeleteArticle(int id)
        {
            var article = _context.Articles.Find(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
