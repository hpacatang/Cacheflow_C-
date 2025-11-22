using ASI.Basecode.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;

        public FeedbackService(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<FeedbackViewModel> GetAllFeedbacks()
        {
            return _repository.GetAllFeedbacks()
                .Select(f => new FeedbackViewModel
                {
                    Id = f.Id,
                    TicketId = f.TicketId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    FeedbackDate = f.FeedbackDate,
                    Status = f.Status
                });
        }

        public FeedbackViewModel GetFeedbackById(int id)
        {
            var feedback = _repository.GetFeedbackById(id);
            if (feedback == null) return null;

            return new FeedbackViewModel
            {
                Id = feedback.Id,
                TicketId = feedback.TicketId,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                FeedbackDate = feedback.FeedbackDate,
                Status = feedback.Status
            };
        }

        public void SubmitFeedback(FeedbackViewModel model)
        {
            var nextId = _repository.GetAllFeedbacks().Max(f => (int?)f.Id) ?? 0;
            nextId++;

            var feedback = new Feedback
            {
                TicketId = model.TicketId,
                Rating = model.Rating,
                Comment = model.Comment,
                FeedbackDate = model.FeedbackDate ?? System.DateTime.Now,
                Status = model.Status ?? "Submitted"
            };

            _repository.AddFeedback(feedback);
            _repository.SaveChanges();
        }
    }
}