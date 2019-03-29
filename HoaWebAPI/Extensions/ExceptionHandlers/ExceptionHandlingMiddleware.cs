using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HoaWebAPI.Extensions.ExceptionHandlers
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IActionResultExecutor<ObjectResult> _executor;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public ExceptionHandlingMiddleware(RequestDelegate next, IActionResultExecutor<ObjectResult> executor, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            _executor = executor;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unhandled exception has occurred while executing the request. " +
                    $"Url: {context.Request.GetDisplayUrl()}. Request Data: " + GetRequestData(context) +
                     $". Error Message: {ex.Message}");

                if (context.Response.HasStarted)
                {
                    throw;
                }

                var routeData = context.GetRouteData() ?? new RouteData();
                var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

                //create an object result so that it can be serialized into XML or JSON depending on content negotiation
                var result = new ObjectResult(new ErrorResponse("Error processing request. Server error. Please try again later."))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };

                await _executor.ExecuteAsync(actionContext, result);
            }
        }

        [DataContract(Name = "ErrorResponse")]
        public class ErrorResponse
        {
            [DataMember(Name = "Message")]
            public string Message { get; set; }

            public ErrorResponse(string message)
            {
                Message = message;
            }
        }

        private static string GetRequestData(HttpContext context)
        {
            var sb = new StringBuilder();

            if (context.Request.HasFormContentType && context.Request.Form.Any())
            {
                sb.Append("Form variables:");
                foreach (var x in context.Request.Form)
                {
                    sb.AppendFormat("Key={0}, Value={1}<br/>", x.Key, x.Value);
                }
            }

            sb.AppendLine("Method: " + context.Request.Method);

            return sb.ToString();
        }

        //Below method is for a simple JSON response for all exceptions. It will always return JSON responses even when XML is requested.
        //private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
        //{
        //    logger.LogError(500, exception, "TEST");
        //    logger.LogInformation(500, exception, "ABCD");

        //    var code = HttpStatusCode.InternalServerError; // 500 if unexpected

        //    if (exception is NullReferenceException || exception is ArgumentNullException)
        //    {
        //        code = HttpStatusCode.BadRequest;
        //    }

        //    //else if (exception is MyUnauthorizedException) code = HttpStatusCode.Unauthorized;
        //    //else if (exception is MyException) code = HttpStatusCode.BadRequest;

        //    var result = JsonConvert.SerializeObject(new { error = "An error occured. Please try again later." });
        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)code;
        //    return context.Response.WriteAsync(result);
        //}
    }
}
