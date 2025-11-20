using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IFeedbackRepository
    {
        IEnumerable<Feedback> GetAllFeedbacks();
        Feedback GetFeedbackById(int id);
        void AddFeedback(Feedback feedback);
        void SaveChanges();
    }
}
