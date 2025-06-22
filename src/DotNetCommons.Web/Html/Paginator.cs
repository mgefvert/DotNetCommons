using System.Text;
using System.Web;

namespace DotNetCommons.Web.Html;

/// <summary>
/// Represents a pagination component for managing navigation within a range of pages.
/// </summary>
public class Paginator
{
    /// <summary>
    /// The current active page number
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// The next page number if available, null if there is no next page
    /// </summary>
    public int? Next { get; set; }

    /// <summary>
    /// The previous page number if available, null if there is no previous page
    /// </summary>
    public int? Previous { get; set; }

    /// <summary>
    /// The maximum page number available
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The middle point used for calculating pagination ranges
    /// </summary>
    public int Mid { get; set; }

    /// <summary>
    /// Collection of pagination links representing the paginator's layout
    /// </summary>
    public List<PaginatorLink> Layout = new();

    /// <summary>
    /// Text to display for ellipsis (truncated page numbers)
    /// </summary>
    public string EllipsisText { get; set; } = "…";

    /// <summary>
    /// Text to display for the 'Next' navigation link
    /// </summary>
    public string NextText { get; set; } = "Next";

    /// <summary>
    /// Text to display for the 'Previous' navigation link
    /// </summary>
    public string PreviousText { get; set; } = "Previous";

    /// <summary>
    /// HTML attribute name used for page selection
    /// </summary>
    public string PageSelectAttribute { get; set; } = "p";

    /// <summary>
    /// CSS class name applied to the main pagination container
    /// </summary>
    public string PaginatorClass { get; set; } = "pagination";

    /// <summary>
    /// CSS class name applied to each pagination list item
    /// </summary>
    public string PaginatorListItemClass { get; set; } = "page-item";

    /// <summary>
    /// CSS class name applied to the active pagination item
    /// </summary>
    public string PaginatorListItemActiveClass { get; set; } = "active";

    /// <summary>
    /// CSS class name applied to pagination links
    /// </summary>
    public string PaginatorLinkClass { get; set; } = "page-link";

    protected Paginator()
    {
    }

    /// <summary>
    /// Calculate a layout for pagination. Assume that we have a sequence of 1..100 pages;
    /// depending on the current page number, we may have "previous" and "next" links to display;
    /// starting page and ending page offset; and a number of pages in the middle indicating the
    /// current selection and surrounding pages.
    ///
    /// For instance, for current=36, count=100, mid=2:
    ///
    ///    &lt;&lt; 0 ..... 34 35 36 37 38 ...... 99 &gt;&gt;
    ///                     ^current
    /// 
    /// </summary>
    /// <param name="currentPage">Current page number, zero-based</param>
    /// <param name="pageCount">Number of pages</param>
    /// <param name="mid">Extra pages in the mid-section, on each side of the current page</param>
    /// <returns>A Paginator object</returns>
    public static Paginator FromPages(int currentPage, int pageCount, int mid)
    {
        var result = new Paginator();
        result.Count = Math.Max(pageCount, 1);
        result.Max = result.Count - 1;
        result.Mid = Math.Max(mid, 0);
        result.Current = Math.Min(Math.Max(currentPage, 0), result.Max);
        result.Previous = null;
        result.Next = null;

        result.Calculate();
        return result;
    }

    /// <summary>
    /// Calculate a layout for pagination. Assume that we have a sequence of 1..100 pages;
    /// depending on the current page number, we may have "previous" and "next" links to display;
    /// starting page and ending page offset; and a number of pages in the middle indicating the
    /// current selection and surrounding pages.
    ///
    /// For instance, for current=36, count=100, mid=2:
    ///
    ///    &lt;&lt; 0 ..... 34 35 36 37 38 ...... 99 &gt;&gt;
    ///                     ^current
    /// 
    /// </summary>
    /// <param name="currentPage">Current page number, zero-based</param>
    /// <param name="itemCount">Total number of objects</param>
    /// <param name="itemsPerPage">Objects per page</param>
    /// <param name="mid">Extra pages in the mid-section, on each side of the current page</param>
    /// <returns>A Paginator object</returns>
    public static Paginator FromItems(int currentPage, int itemCount, int itemsPerPage, int mid)
    {
        var pageCount = Math.Max(itemCount + itemsPerPage - 1, 0) / itemsPerPage;

        return FromPages(currentPage, pageCount, mid);
    }

    private void Calculate()
    {
        var midStart = Math.Max(Current - Mid, 0);
        var midEnd = Math.Min(Current + Mid, Max);

        var result = new List<PaginatorLink>();

        if (Current > 0)
        {
            result.Add(new PaginatorLink(PaginatorLink.Previous, Current - 1));
            Previous = Current - 1;
        }

        if (midStart > 0)
        {
            result.Add(new PaginatorLink(1, 0));
            if (midStart > 1)
                result.Add(new PaginatorLink(PaginatorLink.Ellipsis, null));
        }

        for (var i = midStart; i <= midEnd; i++)
            result.Add(new PaginatorLink(i + 1, i));

        if (midEnd < Max)
        {
            if (midEnd < Max - 1)
                result.Add(new PaginatorLink(PaginatorLink.Ellipsis, null));

            result.Add(new PaginatorLink(Max + 1, Max));
        }

        if (Current < Max)
        {
            result.Add(new PaginatorLink(PaginatorLink.Next, Current + 1));
            Next = Current + 1;
        }

        Layout = result;
    }

    /// <summary>
    /// Renders the paginator object as an HTML unordered list based on the specified CSS class style.
    /// Each list item corresponds to a page link, an ellipsis, or navigation links such as "Previous" and "Next".
    /// </summary>
    /// <returns>A string containing the HTML representation of the paginator.</returns>
    public string Render()
    {
        var paginatorClass      = HttpUtility.HtmlAttributeEncode(PaginatorClass);
        var listItemClass       = HttpUtility.HtmlAttributeEncode(PaginatorListItemClass);
        var listItemActiveClass = HttpUtility.HtmlAttributeEncode(PaginatorListItemActiveClass);
        var linkClass           = HttpUtility.HtmlAttributeEncode(PaginatorLinkClass);

        var ellipsisText = HttpUtility.HtmlEncode(EllipsisText);
        var nextText     = HttpUtility.HtmlEncode(NextText);
        var previousText = HttpUtility.HtmlEncode(PreviousText);

        var result = new StringBuilder();
        result.AppendLine($"<ul class=\"{paginatorClass}\">");

        foreach (var link in Layout)
        {
            switch (link.Display)
            {
                case PaginatorLink.Previous:
                    result.AppendLine($"<li class=\"{listItemClass}\"><a class=\"{linkClass}\" href=\"?{PageSelectAttribute}={link.Page}\">{previousText}</a></li>");
                    break;
                case PaginatorLink.Next:
                    result.AppendLine($"<li class=\"{listItemClass}\"><a class=\"{linkClass}\" href=\"?{PageSelectAttribute}={link.Page}\">{nextText}</a></li>");
                    break;
                case PaginatorLink.Ellipsis:
                    result.AppendLine($"<li class=\"{listItemClass}\"><span>{ellipsisText}</span></li>");
                    break;
                default:
                    {
                        var active = link.Page == Current ? listItemActiveClass : "";
                        result.AppendLine($"<li class=\"{listItemClass} {active}\"><a class=\"{linkClass}\" href=\"?p={link.Page}\">{link.Display}</a></li>");
                        break;
                    }
            }
        }

        result.AppendLine("</ul>");
        return result.ToString();
    }
}