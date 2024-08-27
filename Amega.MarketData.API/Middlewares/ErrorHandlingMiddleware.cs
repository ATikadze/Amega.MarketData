using System.Text.Json;
using Amega.MarketData.Core.DTOs.Response;
using Amega.MarketData.Core.Models.CustomExceptions;
using Microsoft.AspNetCore.Http;

namespace Amega.MarketData.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ILogger<ErrorHandlingMiddleware> logger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            context.Response.ContentType = "application/json";

            string responseMessage;
            var errorMessages = new List<string>();

            try
            {
                if (exception is CustomException ex)
                {
                    context.Response.StatusCode = ex.StatusCode;
                    responseMessage = exception.Message;
                }
                else
                {
                    context.Response.StatusCode = 500;
                    responseMessage = "Unexpected error occured";
                }

                errorMessages.Add(responseMessage);

                logger.LogError(exception.ToString());
            }
            catch (Exception logException)
            {
                errorMessages.Add(logException.Message);
            }

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new ResultResponse { Result = null, ErrorMessages = errorMessages }
                )
            );
        }
    }
}
