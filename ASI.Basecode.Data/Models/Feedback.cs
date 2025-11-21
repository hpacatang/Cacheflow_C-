using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserID { get; set; }
        public int AgentID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? FeedbackDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        public Ticket Ticket { get; set; }
    }
}