namespace DotNetCommons.Web.Html;

public class PaginatorLink
{
    public const int Previous = -1;
    public const int Next = -2;
    public const int Ellipsis = -3;

    /// <summary>
    /// The text or number to display for this pagination link.
    /// Special values: -1 for Previous, -2 for Next, -3 for Ellipsis (...), or the actual page number to display.
    /// </summary>
    public int Display { get; set; }

    /// <summary>
    /// The page number this link points to, or null for non-clickable elements (like ellipsis).
    /// When null, indicates that this element is not clickable in the pagination UI.
    /// </summary>
    public int? Page { get; set; }

    public PaginatorLink(int display, int? page)
    {
        Display = display;
        Page    = page;
    }
}