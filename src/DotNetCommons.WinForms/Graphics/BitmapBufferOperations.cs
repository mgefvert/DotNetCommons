// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms.Graphics;

public static class BitmapBufferOperations
{
    /// <summary>
    /// Removes the background from the bitmap by identifying the most common edge color and adjusting the pixel transparency
    /// based on the similarity to this color.
    /// </summary>
    /// <param name="bitmap">The bitmap buffer to process.</param>
    /// <param name="sensitivity">The sensitivity threshold for determining pixel similarity to the edge color. Higher values allow
    ///     more pixels to be considered part of the background.</param>
    /// <returns>A boolean indicating whether the background removal was successful.</returns>
    public static bool RemoveBackground(this BitmapBuffer bitmap, int sensitivity)
    {
        var w = bitmap.Width - 1;
        var h = bitmap.Height - 1;

        var colors = new[]
        {
            bitmap.GetColor(0, 0), bitmap.GetColor(1, 0),     bitmap.GetColor(0, 1),     bitmap.GetColor(1, 1),
            bitmap.GetColor(w, 0), bitmap.GetColor(w - 1, 0), bitmap.GetColor(w, 1),     bitmap.GetColor(w - 1, 1),
            bitmap.GetColor(0, h), bitmap.GetColor(1, h),     bitmap.GetColor(0, h - 1), bitmap.GetColor(1, h - 1),
            bitmap.GetColor(w, h), bitmap.GetColor(w - 1, h), bitmap.GetColor(w, h - 1), bitmap.GetColor(w - 1, h - 1),
        };

        var candidate = colors
            .GroupBy(x => x)
            .Select(x => new KeyValuePair<Color, int>(x.Key, x.Count()))
            .OrderByDescending(x => x.Value)
            .Select(x => x.Key)
            .ToList();

        if (!candidate.Any())
            return false;

        var color = candidate.First();

        bitmap.ForAllPixels((sl, x, y) =>
        {
            var similarity = Similarity(sl.GetColor(x), color);
            if (similarity < sensitivity)
                sl.SetAlpha(x, (byte)(similarity * (255.0 / sensitivity)));
        });

        return true;
    }

    /// <summary>
    /// Calculates the similarity between two colors using the Euclidean distance in RGB color space.
    /// </summary>
    /// <param name="a">The first color to compare.</param>
    /// <param name="b">The second color to compare.</param>
    /// <returns>A double representing the distance between the two colors. Lower values indicate higher similarity.</returns>
    private static double Similarity(Color a, Color b)
    {
        var dr = a.R - b.R;
        var dg = a.G - b.G;
        var db = a.B - b.B;

        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }
}