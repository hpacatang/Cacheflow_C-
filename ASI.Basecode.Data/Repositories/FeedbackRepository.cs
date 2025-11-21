using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;


namespace ASI.Basecode.Data.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly AsiBasecodeDBContext _context;

        public FeedbackRepository(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        public IEnumerable<Feedback> GetAllFeedbacks()
        {
            return _context.Feedbacks.ToList();
        }

        public Feedback GetFeedbackById(int id)
        {
            return _context.Feedbacks.Find(id);
        }

        public void AddFeedback(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

    }
}