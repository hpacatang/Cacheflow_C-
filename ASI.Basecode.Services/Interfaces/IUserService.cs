using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, ref User user);
        LoginResult RegisterUser(User user);
        IEnumerable<User> GetAllUsers();
    }
}
