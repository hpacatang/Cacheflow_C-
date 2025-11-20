using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class FeedbackViewModel
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserID { get; set; }
        public int AgentID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? FeedbackDate { get; set; }
        public string Status { get; set; }
    }
}
