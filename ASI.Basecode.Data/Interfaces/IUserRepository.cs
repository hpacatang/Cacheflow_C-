using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsers();
        User GetUserByEmail(string email);
        void AddUser(User user);

        // In IUserRepository interface
        void UpdateUser(User user);
    }
}
