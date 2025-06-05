namespace Innago.Shared.ReplaceMe.Handlers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// This is a placeholder class and can be removed.
/// </summary>
public static class Placeholder
{
    /// <summary>
    /// A placeholder stub that can be removed.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task Stub([FromBody] PlaceholderInput input, CancellationToken cancellationToken)
    {
        await Task.Yield();
    }    
}

/// <summary>
/// Input for the placeholder stub.
/// </summary>
/// <param name="Value">the value.</param>
public record PlaceholderInput(string Value);