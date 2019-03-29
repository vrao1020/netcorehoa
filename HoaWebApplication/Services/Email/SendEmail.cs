using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Extensions.Streams;
using HoaWebApplication.Models.EmailModels;
using HoaEntities.Models.OutputModels;
using HoaWebApplication.Services.Cache;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using HoaWebApplication.Services.AccessToken;
using System.Text;
using System.Globalization;
using Microsoft.Extensions.Logging;
using HoaWebApplication.Services.IdentityServer;

namespace HoaWebApplication.Services.Email
{
    public class SendEmail : ISendEmail
    {
        private IConfiguration _configuration;
        private SendGridClient client;
        private ApiClient _apiClient;
        private ICacheStore _cacheStore;
        private IGetAPIAccessToken _getAPIAccessToken;
        private ILogger<SendEmail> _logger;
        private IIDPUserService _iIDPUserService;
        private const string SenderEmail = "hoa@hoa.com";
        private const string SenderEmailName = "Generic HOA";

        public SendEmail(IConfiguration configuration, ApiClient apiClient,
            ICacheStore cacheStore, IGetAPIAccessToken getAPIAccessToken,
            ILogger<SendEmail> logger, IIDPUserService iIDPUserService)
        {
            _configuration = configuration;
            client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
            _apiClient = apiClient;
            _cacheStore = cacheStore;
            _getAPIAccessToken = getAPIAccessToken;
            _logger = logger;
            _iIDPUserService = iIDPUserService;
        }

        public async Task<HttpStatusCode> SendBoardEmailMessageAsync(EmailDto emailDto, bool validFile, string fileType,
            CancellationToken cancellationToken)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress(emailDto.EmailFrom, SenderEmailName),
                Subject = emailDto.Subject,
                HtmlContent = ConstructBoardEmailTemplate(emailDto.Message)
            };

            if (validFile)
            {
                //need to copy the IFormFile into a stream so that it can be attached to the email
                //unlike Azure Blob Storage, there is no method on the SendGrid client to attach a IFormFile directly
                using (var memoryStream = new MemoryStream())
                {
                    await emailDto.Attachment.CopyToAsync(memoryStream, cancellationToken);
                    //set the position to 0 so that the file is not empty
                    memoryStream.Position = 0;

                    //encode the file name
                    var fileName = WebUtility.HtmlEncode(Path.GetFileName(emailDto.Attachment.FileName));
                    //get the extension of the file for the server generated file name
                    var extension = fileName.Substring(fileName.IndexOf("."));

                    await message.AddAttachmentAsync($"Attachment-{DateTime.Now}{extension}", memoryStream, fileType, null, null, cancellationToken);

                }
            }

            var boardEmailList = await _iIDPUserService.GetBoardMemberEmails();

            //initialize a list of email addresses
            List<EmailAddress> messageEmails = new List<EmailAddress>();

            //convert each email fetched from the api and convert to an email address
            foreach (var item in boardEmailList)
            {
                messageEmails.Add(new EmailAddress(item));
            }

            //add the list of board emails to the message
            message.AddTos(messageEmails);

            var response = await client.SendEmailAsync(message, cancellationToken);

            return response.StatusCode;
        }

        public async Task<HttpStatusCode> SendAdminEmailMessageAsync(EmailDto emailDto, CancellationToken cancellationToken)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress(emailDto.EmailFrom, SenderEmailName),
                Subject = emailDto.Subject,
                HtmlContent = ConstructAdminEmailTemplate(emailDto.Message)
            };

            //need to replace this with a list of users who are board members
            message.AddTos(new List<EmailAddress> { new EmailAddress(await _iIDPUserService.GetAdminEmail()) });

            var response = await client.SendEmailAsync(message, cancellationToken);

            return response.StatusCode;
        }

        public async Task SendReminderEmailsAsync()
        {
            //get the cached access token and expiration date if they exist
            var (date, access_token) = await _cacheStore.GetExpirationAccessTokenAsync("hoawebapi");

            //if access token is null or date when last access token was received is longer than 9 hours
            if (access_token == null || ((DateTime.Now - date).TotalHours > 9))
            {
                var new_access_token_ = await _getAPIAccessToken.GetAccessToken();

                //set the access token
                access_token = new_access_token_;

                //set the cache with the item
                await _cacheStore.SetCacheValueAsync("hoawebapi", DateTime.Now.AddHours(9), new_access_token_);
            }

            var baseAPIUrl = $"api/boardmeetings/getcurrentboardmeetingdue";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var apiEventResponse = await _apiClient.SetAuthorization(access_token).GetAsync(baseAPIUrl, HttpCompletionOption.ResponseHeadersRead);

                //ensure success status code else throw an exception
                apiEventResponse.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContentFromApi = await apiEventResponse.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                var meetingFromApi = streamContentFromApi.ReadAndDeserializeFromJson<BoardMeetingViewDto>();

                //only get the users with reminders and send email if an event was returned
                //if no event returned, nothing is scheduled so no need to send any reminders
                if (meetingFromApi != null)
                {
                    //set the api url to fetch the emails if a valid event was returned
                    baseAPIUrl = $"api/users/reminderemails";

                    //get the response with authorization as the API endpoint requires an authenticated user
                    var apiUserResponse = await _apiClient.SetAuthorization(access_token).GetAsync(baseAPIUrl,
                        HttpCompletionOption.ResponseHeadersRead);

                    //ensure success status code else throw an exception
                    apiUserResponse.EnsureSuccessStatusCode();

                    //read the response content into a stream
                    var streamUserContentFromApi = await apiUserResponse.Content.ReadAsStreamAsync();
                    //deserialize the stream into an object (see StreamExtensions on how this is done)
                    var emailList = streamUserContentFromApi.ReadAndDeserializeFromJson<IEnumerable<string>>();

                    //only try to send the email out if the list returned is not empty
                    if (emailList.Count() > 0)
                    {
                        //setup the message to be sent as the list of emails were fetched successfully
                        var message = new SendGridMessage()
                        {
                            From = new EmailAddress(SenderEmail, SenderEmailName),
                            Subject = $"Reminder email for Generic HOA Board Meeting",
                            HtmlContent = ConstructReminderEmailTemplate(meetingFromApi.ScheduledLocation, meetingFromApi.ScheduledTime)
                        };

                        //initialize a list of email addresses
                        List<EmailAddress> messageEmails = new List<EmailAddress>();

                        //convert each email fetched from the api and convert to an email address
                        foreach (var item in emailList)
                        {
                            messageEmails.Add(new EmailAddress(item));
                        }

                        message.AddTos(messageEmails);

                        var responseFromSendGrid = await client.SendEmailAsync(message);

                        if (responseFromSendGrid.StatusCode != HttpStatusCode.Accepted)
                        {
                            //failed to send an email due to the SendGrid service being down or rejecting the request
                            throw new Exception("Website is unavailable at this time");
                        }
                    }
                }

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured while accessing the API." +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }
        }

        public async Task SendUserCreatedEmailAsync(string userName, string userEmail)
        {
            //setup the message to be sent as the list of emails were fetched successfully
            var message = new SendGridMessage()
            {
                From = new EmailAddress(SenderEmail, SenderEmailName),
                Subject = $"New User Created - {userName}",
                HtmlContent = ConstructUserCreatedEmailTemplate(userName, userEmail)
            };

            message.AddTo(await _iIDPUserService.GetAdminEmail());
            var responseFromSendGrid = await client.SendEmailAsync(message);

            if (responseFromSendGrid.StatusCode != HttpStatusCode.Accepted)
            {
                //failed to send an email due to the SendGrid service being down or rejecting the request
                throw new Exception("Website is unavailable at this time");
            }
        }

        private string ConstructBoardEmailTemplate(string emailMessage)
        {
            StringBuilder stringBuilder = new StringBuilder("<div class=\"\"><div class=\"aHl\"></div><div id=\":ub\" tabindex=\"-1\"></div><div id=\":um\" class=\"ii gt\"><div id=\":un\" class=\"a3s aXjCH msg8485945387776152132\"><u></u><div id=\"m_8485945387776152132YIELD_MJML\" style=\"background:#222228\"><div><div class=\"m_8485945387776152132mktEditable\"><div class=\"m_8485945387776152132mj-body\" style=\"background-color:#222228\"><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:20px 0 0 0\"></td></tr><tr><td style=\"font-size:0;padding:10px 25px;padding-bottom:20px\"align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;border-spacing:0px\" align=\"center\"><tbody><tr><td><a href=\"\" target=\"_blank\" data-saferedirecturl=\"\"><img alt=\"test\" src=\"\" style=\"border:none;display:block;outline:none;text-decoration:none;width:100%;max-width:150px;height:auto\" width=\"150\" height=\"auto\" class=\"CToWUd\"></a></td></tr></tbody></table></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:white\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:white\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:30px 40px 0\"><div style=\"vertical-align:top;display:inline-block;font-size:13px;text-align:left;width:100%;min-width:100%\" class=\"m_8485945387776152132mj-column-per-100\" aria-labelledby=\"mj-column-per-100\"><table width=\"100%\"><tbody><tr><td style=\"font-size:0;padding:0 0 20px 0;max-width:480px;overflow:auto\"align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:#555;font-family:avenir,roboto,sans-serif;font-size:14px;line-height:1.8\"><h2 id=\"m_8485945387776152132overview\">Summary</h2><p>");
            stringBuilder.Append(emailMessage);
            stringBuilder.Append("</p></div></td></tr></tbody></table></div></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:0px\"></td></tr><tr><td style=\"font-size:0;padding:20px 30px 10px 30px\" align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:lightgray;font-family:avenir,roboto,sans-serif;font-size:12px;line-height:2\">You are receiving this message because you are part of the Generic HOA Board. This email might contain sensitive information so please do not share with others.</div></td></tr></tbody></table></div></div></div></div></div><div class=\"yj6qo\"></div><div class=\"adL\"></div></div></div><div id=\":u7\" class=\"ii gt\" style=\"display:none\"><div id=\":u6\" class=\"a3s aXjCH undefined\"></div></div><div class=\"hi\"></div></div>");

            var emailTemplate = stringBuilder.ToString();

            return emailTemplate;
        }

        private string ConstructAdminEmailTemplate(string emailMessage)
        {
            StringBuilder stringBuilder = new StringBuilder("<div class=\"\"><div class=\"aHl\"></div><div id=\":ub\" tabindex=\"-1\"></div><div id=\":um\" class=\"ii gt\"><div id=\":un\" class=\"a3s aXjCH msg8485945387776152132\"><u></u><div id=\"m_8485945387776152132YIELD_MJML\" style=\"background:#222228\"><div><div class=\"m_8485945387776152132mktEditable\"><div class=\"m_8485945387776152132mj-body\" style=\"background-color:#222228\"><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:20px 0 0 0\"></td></tr><tr><td style=\"font-size:0;padding:10px 25px;padding-bottom:20px\"align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;border-spacing:0px\" align=\"center\"><tbody><tr><td><a href=\"\" target=\"_blank\" data-saferedirecturl=\"\"><img alt=\"test\" src=\"\" style=\"border:none;display:block;outline:none;text-decoration:none;width:100%;max-width:150px;height:auto\" width=\"150\" height=\"auto\" class=\"CToWUd\"></a></td></tr></tbody></table></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:white\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:white\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:30px 40px 0\"><div style=\"vertical-align:top;display:inline-block;font-size:13px;text-align:left;width:100%;min-width:100%\" class=\"m_8485945387776152132mj-column-per-100\" aria-labelledby=\"mj-column-per-100\"><table width=\"100%\"><tbody><tr><td style=\"font-size:0;padding:0 0 20px 0;max-width:480px;overflow:auto\"align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:#555;font-family:avenir,roboto,sans-serif;font-size:14px;line-height:1.8\"><h2 id=\"m_8485945387776152132overview\">Summary</h2><p>");
            stringBuilder.Append(emailMessage);
            stringBuilder.Append("</p></div></td></tr></tbody></table></div></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:0px\"></td></tr><tr><td style=\"font-size:0;padding:20px 30px 10px 30px\" align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:lightgray;font-family:avenir,roboto,sans-serif;font-size:12px;line-height:2\">You are receiving this message because you are the administrator of the website. This email might contain sensitive information so please do not share with others.</div></td></tr></tbody></table></div></div></div></div></div><div class=\"yj6qo\"></div><div class=\"adL\"></div></div></div><div id=\":u7\" class=\"ii gt\" style=\"display:none\"><div id=\":u6\" class=\"a3s aXjCH undefined\"></div></div><div class=\"hi\"></div></div>");

            var emailTemplate = stringBuilder.ToString();

            return emailTemplate;
        }

        private string ConstructReminderEmailTemplate(string scheduledLocation, DateTime scheduledTime)
        {
            StringBuilder stringBuilder = new StringBuilder("<div class=\"\"><div class=\"aHl\"></div><div id=\":ub\" tabindex=\"-1\"></div><div id=\":um\" class=\"ii gt\"><div id=\":un\" class=\"a3s aXjCH msg8485945387776152132\"><u></u><div id=\"m_8485945387776152132YIELD_MJML\" style=\"background:#222228\"><div><div class=\"m_8485945387776152132mktEditable\"><div class=\"m_8485945387776152132mj-body\" style=\"background-color:#222228\"><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:20px 0 0 0\"></td></tr><tr><td style=\"font-size:0;padding:10px 25px;padding-bottom:20px\"align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;border-spacing:0px\" align=\"center\"><tbody><tr><td><a href=\"\" target=\"_blank\" data-saferedirecturl=\"\"><img alt=\"test\" src=\"\" style=\"border:none;display:block;outline:none;text-decoration:none;width:100%;max-width:150px;height:auto\" width=\"150\" height=\"auto\" class=\"CToWUd\"></a></td></tr></tbody></table></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:white\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:white\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:30px 40px 0\"><div style=\"vertical-align:top;display:inline-block;font-size:13px;text-align:left;width:100%;min-width:100%\" class=\"m_8485945387776152132mj-column-per-100\" aria-labelledby=\"mj-column-per-100\"><table width=\"100%\"><tbody><tr><td style=\"font-size:0;padding:0 0 20px 0;max-width:480px;overflow:auto\"align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:#555;font-family:avenir,roboto,sans-serif;font-size:14px;line-height:1.8\"><h2 id=\"m_8485945387776152132overview\">HOA Board Meeting</h2><p>");
            stringBuilder.Append($"A HOA Board Meeting is occuring tomorrow. If you are attending, please be sure to arrive on time. <h4 style=\"margin: 0 0 0 0\">Scheduled Time: {scheduledTime.ToString("MMMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture)}</h4> <h4 style=\"margin: 0 0 0 0\">Scheduled Location: {scheduledLocation}</h4>");
            stringBuilder.Append("</p></div></td></tr></tbody></table></div></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:0px\"></td></tr><tr><td style=\"font-size:0;padding:20px 30px 10px 30px\" align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:lightgray;font-family:avenir,roboto,sans-serif;font-size:12px;line-height:2\">You are receiving this message because you signed up to receive notifications. If you no longer want to receive notifications, you can update your preferences in your profile by logging in to the website.</div></td></tr></tbody></table></div></div></div></div></div><div class=\"yj6qo\"></div><div class=\"adL\"></div></div></div><div id=\":u7\" class=\"ii gt\" style=\"display:none\"><div id=\":u6\" class=\"a3s aXjCH undefined\"></div></div><div class=\"hi\"></div></div>");

            var emailTemplate = stringBuilder.ToString();

            return emailTemplate;
        }

        private string ConstructUserCreatedEmailTemplate(string userName, string userEmail)
        {
            StringBuilder stringBuilder = new StringBuilder("<div class=\"\"><div class=\"aHl\"></div><div id=\":ub\" tabindex=\"-1\"></div><div id=\":um\" class=\"ii gt\"><div id=\":un\" class=\"a3s aXjCH msg8485945387776152132\"><u></u><div id=\"m_8485945387776152132YIELD_MJML\" style=\"background:#222228\"><div><div class=\"m_8485945387776152132mktEditable\"><div class=\"m_8485945387776152132mj-body\" style=\"background-color:#222228\"><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:20px 0 0 0\"></td></tr><tr><td style=\"font-size:0;padding:10px 25px;padding-bottom:20px\"align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;border-spacing:0px\" align=\"center\"><tbody><tr><td><a href=\"\" target=\"_blank\" data-saferedirecturl=\"\"><img alt=\"auth0\" src=\"https://i.imgur.com/qB6Ja3E.jpg\" style=\"border:none;display:block;outline:none;text-decoration:none;width:100%;max-width:150px;height:auto\" width=\"150\" height=\"auto\" class=\"CToWUd\"></a></td></tr></tbody></table></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:white\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:white\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:30px 40px 0\"><div style=\"vertical-align:top;display:inline-block;font-size:13px;text-align:left;width:100%;min-width:100%\" class=\"m_8485945387776152132mj-column-per-100\" aria-labelledby=\"mj-column-per-100\"><table width=\"100%\"><tbody><tr><td style=\"font-size:0;padding:0 0 20px 0;max-width:480px;overflow:auto\"align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:#555;font-family:avenir,roboto,sans-serif;font-size:14px;line-height:1.8\"><h2 id=\"m_8485945387776152132overview\">Summary</h2><p>");
            stringBuilder.Append($"A new user - {userName} ({userEmail}) has been created. Please provide them with the appropriate access level.");
            stringBuilder.Append("</p></div></td></tr></tbody></table></div></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:0px\"></td></tr><tr><td style=\"font-size:0;padding:20px 30px 10px 30px\" align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:lightgray;font-family:avenir,roboto,sans-serif;font-size:12px;line-height:2\">You are receiving this message because you are an administrator of the website.</div></td></tr></tbody></table></div></div></div></div></div><div class=\"yj6qo\"></div><div class=\"adL\"></div></div></div><div id=\":u7\" class=\"ii gt\" style=\"display:none\"><div id=\":u6\" class=\"a3s aXjCH undefined\"></div></div><div class=\"hi\"></div></div>");

            var emailTemplate = stringBuilder.ToString();

            return emailTemplate;
        }
    }
}
