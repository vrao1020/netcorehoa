using HoaWebApplication.Services.Azure;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HoaWebApplication.ViewComponents
{
    public class MeetingMinuteListAllViewComponent: ViewComponent
    {
        private IAzureBlob _azureBlob;

        public MeetingMinuteListAllViewComponent(IAzureBlob azureBlob)
        {
            _azureBlob = azureBlob;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //fetcht he list of files from the azure container
            var items = await _azureBlob.GetAzureBlobAllDirectoryFileReferencesAsync("meetingminutes");            
            return View(items);
        }

    }
}
