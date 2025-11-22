using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        // identifier can be email or name
        LoginResult AuthenticateUser(string identifier, string password, ref User user);
        LoginResult RegisterUser(User user);
        IEnumerable<User> GetAllUsers();
        bool UpdateUser(User user);
    }
}
