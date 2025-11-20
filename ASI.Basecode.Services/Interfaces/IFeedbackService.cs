using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IFeedbackService
    {
        IEnumerable<FeedbackViewModel> GetAllFeedbacks();
        FeedbackViewModel GetFeedbackById(int id);
        void SubmitFeedback(FeedbackViewModel model);
    }
}
