namespace DotNetCommons;

public static class CommonUriExtensions
{
    /// <summary>
    /// Adds or replaces the query string of the specified URI with the given query.
    /// </summary>
    /// <param name="uri">The original URI to which the query string will be applied.</param>
    /// <param name="query">The query string to add or replace in the URI. If null or empty, the query string will be cleared.</param>
    /// <returns>A new <see cref="Uri"/> instance with the manipulated query string.</returns>
    public static Uri WithQuery(this Uri uri, string query)
    {
        ArgumentNullException.ThrowIfNull(uri);

        var path = uri.GetLeftPart(UriPartial.Path);
        return query.IsSet()
            ? new Uri(path + "?" + query)
            : new Uri(path);
    }

    /// <summary>
    /// Adds or replaces the query string of the specified URI with the provided query parameters.
    /// </summary>
    /// <param name="uri">The original URI to which the query parameters will be applied.</param>
    /// <param name="queryParameters">A collection of key-value pairs representing the query parameters to add or replace. Parameters with
    /// null values will be ignored.</param>
    /// <returns>A new <see cref="Uri"/> instance with the manipulated query string.</returns>
    public static Uri WithQuery(this Uri uri, IEnumerable<KeyValuePair<string, string?>> queryParameters)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(queryParameters);

        var query = string.Join("&", queryParameters
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!)}"));

        return WithQuery(uri, query);
    }
}