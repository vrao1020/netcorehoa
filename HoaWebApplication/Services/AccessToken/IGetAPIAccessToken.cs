using System.Threading.Tasks;

namespace HoaWebApplication.Services.AccessToken
{
    public interface IGetAPIAccessToken
    {
        Task<string> GetAccessToken();
    }
}
