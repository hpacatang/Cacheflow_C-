using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
        }

        /// <summary>
        /// Login Method
        /// </summary>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)

        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new User();
            var result = _userService.AuthenticateUser(model.UserId, model.Password, ref user);
            if (result == ASI.Basecode.Resources.Constants.Enums.LoginResult.Success)
            {
                // create claims from authenticated user and sign in
                var identity = _signInManager.CreateClaimsIdentity(user);
                if (identity != null)
                {
                    var principal = _signInManager.CreateClaimsPrincipal(identity);
                    await _signInManager.SignInAsync(principal);

                    this._session.SetString("HasSession", "Exist");
                    this._session.SetString("UserName", model.UserId);

                    return Ok(new { success = true, user = user });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid credentials" });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ASI.Basecode.Services.ServiceModels.UserViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new User
            {
                UserId = model.UserId,
                Name = model.Name,
                Password = model.Password,
                Email = null,
                Role = "User",
                Status = "Active"
            };

            var result = _userService.RegisterUser(user);
            if (result == ASI.Basecode.Resources.Constants.Enums.LoginResult.Success)
            {
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false, message = "Registration failed" });
        }

        /// <summary>
        /// Sign Out current account
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return Ok();
        }
    }
}
