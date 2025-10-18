using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public User GetUserByEmail(string email)
        {
            return this.GetDbSet<User>().FirstOrDefault(u => u.Email == email);
        }

        public void AddUser(User user)
        {
            var dbSet = this.GetDbSet<User>();
            dbSet.Add(user);
            // Persist changes using UnitOfWork
            UnitOfWork.SaveChanges();
        }

    }
}
