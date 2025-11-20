using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class TicketWithFeedbackViewModel
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Name { get; set; }
        public string Assignee { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }

        public List<FeedbackViewModel> Feedback { get; set; }
    }
}
