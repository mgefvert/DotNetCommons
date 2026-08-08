using System.Net.Http.Json;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonHttpClientExtensions
{
    public static async Task<T> GetCommon<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>()
               ?? throw new Exception($"Failed to deserialize response in GET {url.GetSubItem('?', 0)}");
    }

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