namespace Innago.Shared.ReplaceMe;

internal static partial class LoggerMessages
{
    [LoggerMessage(LogLevel.Information, "{Json} - {Response}")]
    public static partial void LogExternalCall(this ILogger logger, string json, string? response);
}