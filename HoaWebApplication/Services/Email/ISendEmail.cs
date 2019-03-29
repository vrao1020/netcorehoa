using HoaWebApplication.Models.EmailModels;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HoaWebApplication.Services.Email
{
    public interface ISendEmail
    {
        Task<HttpStatusCode> SendBoardEmailMessageAsync(EmailDto emailDto, bool validFile, string fileType,
            CancellationToken cancellationToken);
        Task<HttpStatusCode> SendAdminEmailMessageAsync(EmailDto emailDto, CancellationToken cancellationToken);
        Task SendReminderEmailsAsync();
        Task SendUserCreatedEmailAsync(string userName, string userEmail);
    }
}
