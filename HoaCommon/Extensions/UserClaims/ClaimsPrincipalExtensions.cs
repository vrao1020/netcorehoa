using System.Linq;
using System.Security.Claims;

namespace HoaCommon.Extensions.UserClaims
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Fetch the user based on the social Id present in the JWT. Returns null if user is not found
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetUserId(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims.Count() == 0)
            {
                return null;
            }

            var authenticatedUser = user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return authenticatedUser;
        }

        /// <summary>
        /// Returns true if the user is an administrator. This is based on a claim being present in the JWT.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdministrator(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims.Count() == 0)
            {
                return false;
            }

            var role = user?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

            if (role == "admin")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the current authenticated user is the owner of the object
        /// Parameter socialId is the owner social Id of the object being tested against
        /// </summary>
        /// <param name="user"></param>
        /// <param name="socialId"></param>
        /// <returns></returns>
        public static bool IsObjectOwner(this ClaimsPrincipal user, string currentUserSocialId, string socialId)
        {
            if (user == null || user.Claims.Count() == 0)
            {
                return false;
            }

            if (socialId == currentUserSocialId)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the current authenticated user has CRUD access
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool HasCRUDAccess(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims.Count() == 0)
            {
                return false;
            }

            string crudAccess = "readonly";
            var accessType = user?.Claims?.FirstOrDefault(x => x.Type == crudAccess)?.Value;

            if (accessType == "false")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the current authenticated user has CRUD access
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool HasPostCreationAccess(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims.Count() == 0)
            {
                return false;
            }

            string postAccess = "postcreation";
            var accessType = user?.Claims?.FirstOrDefault(x => x.Type == postAccess)?.Value;

            if (accessType == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
