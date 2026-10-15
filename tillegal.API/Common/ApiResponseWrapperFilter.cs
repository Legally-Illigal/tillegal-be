using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tillegal.API.Models;

namespace Tillegal.API.Common
{
    public class ApiResponseWrapperFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context, 
            ResultExecutionDelegate next)
        {
            switch (context.Result)
            {
                case ObjectResult { Value: ApiResponse }:
                    break;

                case ObjectResult objectResult:
                    objectResult.Value = Build(objectResult.Value, objectResult.StatusCode ?? StatusCodes.Status200OK);
                    break;

                case StatusCodeResult statusCodeResult:
                    context.Result = new ObjectResult(Build(null, statusCodeResult.StatusCode))
                    {
                        StatusCode = statusCodeResult.StatusCode
                    };
                    break;

                case EmptyResult:
                    context.Result = new ObjectResult(Build(null, StatusCodes.Status200OK))
                    {
                        StatusCode = StatusCodes.Status200OK
                    };
                    break;
            }

            await next();
        }

        private static ApiResponse Build(object? value, int statusCode)
        {
            if (statusCode is >= 200 and < 300)
            {
                return ApiResponse.Success(value, (HttpStatusCode)statusCode);
            }

            return ApiResponse.Fail((HttpStatusCode)statusCode, ExtractMessages(value, statusCode));
        }

        private static string[] ExtractMessages(object? value, int statusCode)
        {
            return value switch
            {
                ValidationProblemDetails vpd => vpd.Errors.SelectMany(kvp => kvp.Value).ToArray(),
                ProblemDetails pd => new[] { pd.Detail ?? pd.Title ?? DefaultMessage(statusCode) },
                string s => new[] { s },
                null => new[] { DefaultMessage(statusCode) },
                _ => new[] { value.ToString()! }
            };
        }

        private static string DefaultMessage(int statusCode) =>
            ((HttpStatusCode)statusCode).ToString();
    }
}