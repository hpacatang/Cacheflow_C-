using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class TicketRepository : ITicketRepository
    {
   private readonly AsiBasecodeDBContext _context;

        public TicketRepository(AsiBasecodeDBContext context)
        {
      _context = context;
        }

  public Ticket? GetTicketWithFeedback(int id)
        {
      return _context.Tickets
         .Include(t => t.Feedback)
      .FirstOrDefault(t => t.Id == id);
        }
    }
}
