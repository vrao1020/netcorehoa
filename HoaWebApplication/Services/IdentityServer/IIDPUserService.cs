using HoaEntities.Models.OutputModels;
using HoaWebApplication.Models.PaginationModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoaWebApplication.Services.IdentityServer
{
    public interface IIDPUserService
    {
        /// <summary>
        /// Returns a paged list of users. Paged list is an extension of list that contains total records found in Auth0 users
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(IEnumerable<IDPUserViewDto>, XPaginationHeaderDto)> GetAllUsersAsync(int pageNum, int pageSize);
        Task<IDPUserViewDto> GetUserAsync(string userId);
        Task<string> GetUserManagementAccessTokenAsync();
        Task UpdateUser(bool ReadOnlyAccess, string Role, bool PostCreation, string id);
        Task<IEnumerable<string>> GetBoardMemberEmails();
        Task<string> GetAdminEmail();
    }
}
