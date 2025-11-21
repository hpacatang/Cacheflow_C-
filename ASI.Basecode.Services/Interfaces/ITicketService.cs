using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITicketService
    {
        Ticket Create(Ticket ticket, IFormFile? attachment);
        IEnumerable<Ticket> GetAll();
        Ticket? Get(int id);
        bool Update(int id, string? jsonData, IFormFile? attachment);
        bool Delete(int id);
        bool DeleteAttachment(int id);
        TicketWithFeedbackViewModel GetTicketWithFeedback(int id);
        IEnumerable<object> GetAllWithFeedback();
    }
}