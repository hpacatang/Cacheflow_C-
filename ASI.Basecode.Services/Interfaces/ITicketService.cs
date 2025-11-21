using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITicketService
    {
        TicketWithFeedbackViewModel GetTicketWithFeedback(int id);
    }
}