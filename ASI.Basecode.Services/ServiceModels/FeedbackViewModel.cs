using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class FeedbackViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "TicketId is required.")]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "UserID is required.")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "AgentID is required.")]
        public int AgentID { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; }

        public DateTime? FeedbackDate { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; }
    }
}