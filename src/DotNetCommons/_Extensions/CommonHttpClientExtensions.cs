using System.Net.Http.Json;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

/// <summary>
/// Provides convenience methods for sending HTTP requests and reading JSON responses.
/// </summary>
public static class CommonHttpClientExtensions
{
    /// <summary>
    /// Sends a GET request and deserializes the JSON response body.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="url">The request URI.</param>
    /// <returns>The response body deserialized as <typeparamref name="T"/>.</returns>
    /// <exception cref="HttpRequestException">The server response indicates a failure.</exception>
    /// <exception cref="Exception">The response body could not be deserialized to a non-null value.</exception>
    public static async Task<T> GetCommon<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>()
               ?? throw new Exception($"Failed to deserialize response in GET {url.GetSubItem('?', 0)}");
    }

    /// <summary>
    /// Sends a POST request and deserializes the JSON response body.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="url">The request URI.</param>
    /// <param name="content">The request body to serialize as JSON, or <see langword="null"/> to send an empty body.</param>
    /// <returns>The response body deserialized as <typeparamref name="T"/>.</returns>
    /// <exception cref="HttpRequestException">The server response indicates a failure.</exception>
    /// <exception cref="Exception">The response body could not be deserialized to a non-null value.</exception>
    public static async Task<T> PostCommon<T>(this HttpClient client, string url, object? content = null)
    {
        var response = content == null
            ? await client.PostAsync(url, null)
            : await client.PostAsJsonAsync(url, content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>()
               ?? throw new Exception($"Failed to deserialize response in POST {url.GetSubItem('?', 0)}");
    }
}
