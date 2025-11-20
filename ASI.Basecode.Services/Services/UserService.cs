using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        // identifier can be email or name
        public LoginResult AuthenticateUser(string identifier, string password, ref User user)
        {
            user = null;
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers()
                              .Where(x => (x.Email == identifier || x.Name == identifier) &&
                                           x.Password == passwordKey)
                              .FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public LoginResult RegisterUser(User user)
        {
            // Ensure required fields
            if (user == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Password))
                return LoginResult.Failed;

            // Check duplicate by name or email
            var existing = _repository.GetUsers().Any(x => x.Name == user.Name || (user.Email != null && x.Email == user.Email));
            if (existing) return LoginResult.Failed;

            // Encrypt password before storing
            user.Password = PasswordManager.EncryptPassword(user.Password);
            user.CreatedTime = System.DateTime.UtcNow;
            user.UpdatedTime = System.DateTime.UtcNow;
            user.CreatedBy = user.Name;
            user.UpdatedBy = user.Name;

            _repository.AddUser(user);
            return LoginResult.Success;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _repository.GetUsers().ToList();
        }
    }
}
