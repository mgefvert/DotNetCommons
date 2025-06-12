using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using DotNetCommons.Net;
using Microsoft.AspNetCore.Html;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Web.Html;

public static class HtmlTools
{
    /// <summary>
    /// Darkens a color by a given percentage.
    /// </summary>
    /// <param name="color">The color to be darkened.</param>
    /// <param name="pct">The percentage to darken the color. Negative values will lighten the color.</param>
    /// <returns>A new <see cref="Color"/> instance that is the darkened version of the input color.</returns>
    public static Color Darken(this Color color, float pct) => Lighten(color, -pct);

    /// <summary>
    /// Generates a Gravatar URL based on the provided email and optional parameters.
    /// </summary>
    /// <param name="email">The email address associated with the Gravatar.</param>
    /// <param name="size">Optional. The size of the Gravatar image in pixels. Valid values are between 1 and 2048.</param>
    /// <param name="mode">Optional. The default image mode to use if no Gravatar is found for the email.
    ///                    Examples include "404", "mm", "identicon".</param>
    /// <param name="rating">Optional. The rating of the Gravatar image. Examples include "g", "pg", "r", or "x".</param>
    /// <returns>The fully constructed Gravatar URL.</returns>
    public static string GravatarUrl(string email, int? size = null, string? mode = null, string? rating = null)
    {
        using var md5 = MD5.Create();

        var data = Encoding.UTF8.GetBytes(email.Trim().ToLower());
        var hash = md5.ComputeString(data);

        var parameters = new Dictionary<string, string>();

        if (size != null)
            parameters["s"] = size.ToString()!;
        if (mode != null)
            parameters["d"] = mode;
        if (rating != null)
            parameters["r"] = rating;

        return CommonWebClient.EncodeQuery("https://www.gravatar.com/avatar/" + hash, parameters);
    }

    /// <summary>
    /// Lightens a color by a given percentage.
    /// </summary>
    /// <param name="color">The color to be lightened.</param>
    /// <param name="pct">The percentage to lighten the color. Negative values will darken the color.</param>
    /// <returns>A new <see cref="Color"/> instance that is the lightened version of the input color.</returns>
    public static Color Lighten(this Color color, float pct)
    {
        pct /= 100;

        var r = (int)(255 * pct + color.R * (1 - pct));
        var g = (int)(255 * pct + color.G * (1 - pct));
        var b = (int)(255 * pct + color.B * (1 - pct));

        return Color.FromArgb(r, g, b);
    }

    /// <summary>
    /// Converts newline characters in a string to HTML line break elements (&lt;br&gt;).
    /// </summary>
    /// <param name="text">The input string containing newline characters. If null or empty, the input is returned as-is.</param>
    /// <returns>An <see cref="HtmlString"/> with newline characters replaced by HTML line break elements.</returns>
    public static HtmlString Nl2Br(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return new HtmlString(text);

        return new HtmlString(text
            .Replace("\r\n", "<br>")
            .Replace("\n", "<br>")
            .Replace("\r", ""));
    }

    /// <summary>
    /// Converts a <see cref="Color"/> object to its CSS hex representation.
    /// </summary>
    /// <param name="color">The color to convert to a CSS hex string.</param>
    /// <returns>A CSS-compatible hex string representing the color.</returns>
    public static string ToCss(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Converts a <see cref="Color"/> object to its CSS hex representation, including the Alpha transparency value if needed.
    /// </summary>
    /// <param name="color">The color to convert to a CSS hex string.</param>
    /// <returns>A CSS-compatible hex string representing the color.</returns>
    public static string ToCssAlpha(this Color color)
    {
        return color.A == 255
            ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }
}