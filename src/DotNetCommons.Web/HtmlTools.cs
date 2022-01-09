using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using DotNetCommons.Net;
using DotNetCommons.Security;
using Microsoft.AspNetCore.Html;

namespace DotNetCommons.Web
{
    public static class HtmlTools
    {
        public static Color Darken(this Color color, float pct) => Lighten(color, -pct);

        public static string GravatarUrl(string email, int? size = null, string mode = null, string rating = null)
        {
            using var md5 = MD5.Create();

            var data = Encoding.UTF8.GetBytes(email.Trim().ToLower());
            var hash = md5.ComputeString(data);

            var parameters = new Dictionary<string, string>();

            if (size != null)
                parameters["s"] = size.ToString();
            if (mode != null)
                parameters["d"] = mode;
            if (rating != null)
                parameters["r"] = rating;

            return CommonWebClient.EncodeQuery("https://www.gravatar.com/avatar/" + hash, parameters);
        }

        public static Color Lighten(this Color color, float pct)
        {
            pct /= 100;

            var r = (int)(255 * pct + color.R * (1 - pct));
            var g = (int)(255 * pct + color.G * (1 - pct));
            var b = (int)(255 * pct + color.B * (1 - pct));

            return Color.FromArgb(r, g, b);
        }

        public static HtmlString Nl2Br(string text)
        {
            return new HtmlString(text
                .Replace("\r\n", "<br>")
                .Replace("\n", "<br>")
                .Replace("\r", ""));
        }

        public static string ToCss(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
