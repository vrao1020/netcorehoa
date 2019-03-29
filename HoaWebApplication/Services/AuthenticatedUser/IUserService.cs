using HoaEntities.Models.InputModels;
using System.Threading;
using System.Threading.Tasks;

namespace HoaWebApplication.Services.AuthenticatedUser
{
    public interface IUserService
    {
        Task<(bool userCreated, string UserName, string UserEmail)> CreateUpdateUserAsync(CancellationToken cancellationToken);
        Task<bool> UserExistsAsync(CancellationToken cancellationToken);
        Task<(string userName, string userEmail)> CreateUserAsync(CancellationToken cancellationToken);
        UserInputDto GetUserForCreation();
        bool ValidEmail();
        bool IsSocialLogin();
    }
}
