using Projects.Models;
using Capsitech.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Principal;
//using static iText.StyledXmlParser.Jsoup.Select.Evaluator;


namespace Projects.Identity
{
    /// <summary>
    /// Provides the APIs for user sign in.
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser>
    {
        /// <summary>
        /// Creates a new instance of <see cref="SignInManager{TUser}"/>.
        /// </summary>
        /// <param name="userManager">An instance of <see cref="UserManager{ApplicationUser}"/> used to retrieve users from and persist users.</param>
        /// <param name="contextAccessor">The accessor used to access the <see cref="HttpContext"/>.</param>
        /// <param name="claimsFactory">The factory to use to create claims principals for a user.</param>
        /// <param name="optionsAccessor">The accessor used to access the <see cref="IdentityOptions"/>.</param>
        /// <param name="logger">The logger used to log messages, warnings and errors.</param>
        /// <param name="schemes">The scheme provider that is used enumerate the authentication schemes.</param>
        public ApplicationSignInManager(UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, null) { }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> for the specified <paramref name="user"/>, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to create a <see cref="ClaimsPrincipal"/> for.</param>
        /// <returns>The task object representing the asynchronous operation, containing the ClaimsPrincipal for the specified user.</returns>
        public override async Task<ClaimsPrincipal> CreateUserPrincipalAsync(ApplicationUser user)
        {
            ClaimsPrincipal principal = await ClaimsFactory.CreateAsync(user);

            // use this.UserManager if needed
            var identity = (ClaimsIdentity)principal.Identity;
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Name.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.SerialNumber, user.Company.Id));

            if (!user.GetImageThumb().IsEmpty())
                identity.AddClaim(new Claim("ThumbPath", user.GetImageThumb()));

            if (user.Company != null)
                identity.AddClaim(new Claim("CompanyName", string.Join(",", user.Company.Name)));
                       
            return principal;
        }

        /// <summary>
        /// Create a JwtToken to authenticate APIs
        /// </summary>
        /// <param name="user">The user to create a token for</param>
        /// <returns>JwtToken</returns>
        public string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, user.Name.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName)
                //new Claim(ClaimTypes.SerialNumber, user.Company.Id)
            };

            if (user.Roles?.Count > 0)
                claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            if (user.Company != null)
                claims.Add(new Claim("CompanyName", string.Join(",", user.Company.Name)));

            //if (user.Branch != null)
            //    claims.Add(new Claim("BranchId", string.Join(",", user.Branch.Id)));

            if (user.Role != null)
                claims.Add(new Claim("UserRole", string.Join(",", user.Role.ToString())));

            //generate token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConfig.Current.Jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: AppConfig.Current.Jwt.Issuer,
                //audience: config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddDays(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
