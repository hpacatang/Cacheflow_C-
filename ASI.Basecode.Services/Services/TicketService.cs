using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public TicketWithFeedbackViewModel GetTicketWithFeedback(int id)
        {
            var ticket = _ticketRepository.GetTicketWithFeedback(id);

            if (ticket == null)
                return null;

            return new TicketWithFeedbackViewModel
            {
                Id = ticket.Id,
                Summary = ticket.Summary,
                UserID = ticket.UserID,
                AgentID = ticket.AgentID,
                Type = ticket.Type,
                Description = ticket.Description,
                DueDate = ticket.DueDate,
                ResolvedAt = ticket.ResolvedAt,
                Priority = ticket.Priority,
                Category = ticket.Category,
                CreatedTime = ticket.CreatedTime,
                CreatedBy = ticket.CreatedBy,
                UpdatedTime = ticket.UpdatedTime,
                UpdatedBy = ticket.UpdatedBy,
                Feedback = ticket.Feedback.Select(f => new FeedbackViewModel
                {
                    Id = f.Id,
                    TicketId = f.TicketId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    FeedbackDate = f.FeedbackDate,
                    Status = f.Status
                }).ToList()
            };
        }
    }
}