using HoaEntities.Models.UpdateModels;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HoaWebApplication.ApiHttpClient
{
    public static class UserPatchExtension
    {
        /// <summary>
        /// Updates the current user's reminder for receiving emails
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PatchUserReminderEmailJsonAsync(this HttpClient httpClient,
            string requestUrl, bool value, string email, CancellationToken cancellationToken)
        {
            // create a JsonPatch document 
            JsonPatchDocument<UserUpdateDto> patchDoc = new JsonPatchDocument<UserUpdateDto>();
            patchDoc.Replace(b => b.Reminder, value);

            //only replace email if the value is not null
            if (email != null)
            {
                patchDoc.Replace(b => b.Email, email);
            }


            // serialize
            var serializedPatchDoc = JsonConvert.SerializeObject(patchDoc);

            // create the patch request
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUrl)
            {
                Content = new StringContent(serializedPatchDoc,
                System.Text.Encoding.Unicode, "application/json")
            };

            return await httpClient.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Updates the current user's first name, last name
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUrl"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PatchUserNameJsonAsync(this HttpClient httpClient,
            string requestUrl, string firstName, string lastName, CancellationToken cancellationToken)
        {
            // create a JsonPatch document 
            JsonPatchDocument<UserUpdateDto> patchDoc = new JsonPatchDocument<UserUpdateDto>();
            patchDoc.Replace(b => b.FirstName, firstName);
            patchDoc.Replace(b => b.LastName, lastName);

            // serialize
            var serializedPatchDoc = JsonConvert.SerializeObject(patchDoc);

            // create the patch request
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUrl)
            {
                Content = new StringContent(serializedPatchDoc,
                System.Text.Encoding.Unicode, "application/json")
            };

            return await httpClient.SendAsync(request, cancellationToken);
        }
    }
}
