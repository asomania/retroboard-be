namespace Retroboard.Api.Api.Models;

public class ApiErrorResponse
{
    public bool Success { get; } = false;
    public string Message { get; }
    public IDictionary<string, string[]> Errors { get; }

    public ApiErrorResponse(string message, IDictionary<string, string[]> errors)
    {
        Message = message;
        Errors = errors;
    }

    public ApiErrorResponse(string message)
        : this(message, new Dictionary<string, string[]>())
    {
    }
}
