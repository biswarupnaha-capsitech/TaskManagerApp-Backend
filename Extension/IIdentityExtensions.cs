using Capsitech.Extensions;
using Capsitech.Utility;
using System.Security.Claims;
using System.Security.Principal;

namespace Projects.Extensions
{
    public static class IIdentityExtensions
    {
       
        /// <summary>
        /// Get companyId of the current user
        /// </summary>
        /// <param name="principal">Current user principal</param>
        /// <returns>string</returns>
        public static string GetCompanyId(this ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated == true)
                return principal.FindFirst(ClaimTypes.SerialNumber)?.Value;
            
            return "";
        }

        /// <summary>
        /// Get companyId of the current user
        /// </summary>
        /// <param name="identity">Current user principal</param>
        /// <returns>string</returns>
        public static string GetCompanyName(this IIdentity identity)
        {
            if (identity?.IsAuthenticated == true)
            {
                Claim c = ((ClaimsIdentity)identity).FindFirst("CompanyName");
                if (c != null && !c.Value.IsEmpty())
                    return c.Value;
            }

            return "";
        }
        /// <summary>
        /// Get user role of the current user
        /// </summary>
        /// <param name="identity">Current user principal</param>
        /// <returns>string</returns>
        public static string GetUserRole(this IIdentity identity)
        {
            if (identity?.IsAuthenticated == true)
            {
                Claim c = ((ClaimsIdentity)identity).FindFirst("UserRole");
                if (c != null && !c.Value.IsEmpty())
                    return c.Value;
            }

            return "";
        }

        /// <summary>
        /// Get branchId of the current user
        /// </summary>
        /// <param name="identity">Current user principal</param>
        /// <returns>string</returns>
        public static string GetBranchId(this IIdentity identity)
        {
            if (identity?.IsAuthenticated == true)
            {
                Claim c = ((ClaimsIdentity)identity).FindFirst("BranchId");
                if (c != null && !c.Value.IsEmpty())
                    return c.Value;
            }

            return "";
        }
    }
}