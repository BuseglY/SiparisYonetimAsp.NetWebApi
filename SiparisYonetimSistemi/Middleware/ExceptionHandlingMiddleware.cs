using System.Net;
using System.Text.Json;

namespace SiparisYonetimSistemi.Middleware
{
        public class ExceptionHandlingMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<ExceptionHandlingMiddleware> _logger;

            public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred");
                    await HandleExceptionAsync(context, ex);
                }
            }

            private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = new
                    {
                        message = "Bir hata oluştu.",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    }
                };

                switch (exception)
                {
                    case ArgumentException:
                    case InvalidOperationException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response = new
                        {
                            error = new
                            {
                                message = exception.Message,
                                details = exception.Message,
                                timestamp = DateTime.UtcNow
                            }
                        };
                        break;

                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }