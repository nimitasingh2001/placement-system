using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace PlacementApi.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail, code) = Map(exception);

        if (status >= 500)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning("Request rejected ({Code}): {Detail}", code, detail);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        if (code is not null)
            problem.Extensions["errorCode"] = code;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }

    private static (int Status, string Title, string Detail, int? Code) Map(Exception exception)
    {
        var sql = FindSqlException(exception);
        if (sql is null)
            return (500, "Server error", "Something went wrong on our side. Please try again later.", null);

        var code = sql.Number;
        return code switch
        {
            // our THROW messages in the stored procedures are written for people, so they are safe to show
            50001 or 50003 or 50010 or 50020
                => (404, "Not found", sql.Message, code),
            50007 or 50011 or 50012 or 50023 or 50024
                => (409, "Conflict", sql.Message, code),
            50002 or 50004 or 50005 or 50006 or 50014 or 50021 or 50022
                => (400, "Request not allowed", sql.Message, code),

            // built-in SQL Server errors: show a generic message, not the raw text
            2627 or 2601
                => (409, "Conflict", "A record with the same unique value already exists.", code),
            547
                => (400, "Invalid data", "The data breaks a database rule (for example a missing related record or an invalid value).", code),

            _ => (500, "Server error", "Something went wrong on our side. Please try again later.", code)
        };
    }

    // EF Core wraps the SQL error inside other exceptions, so walk down the chain
    private static SqlException? FindSqlException(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is SqlException sql) return sql;
            exception = exception.InnerException;
        }
        return null;
    }
}