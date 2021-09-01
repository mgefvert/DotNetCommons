using System;
using System.Collections.Generic;
using System.Text;
using DotNetCommons.Html;

namespace DotNetCommons.Web
{
    public class PaginatorLink
    {
        public const int Previous = -1;
        public const int Next = -2;
        public const int Ellipsis = -3;

        public int Display { get; set; }
        public int? Page { get; set; }

        public PaginatorLink(int display, int? page)
        {
            Display = display;
            Page = page;
        }
    }

    public class Paginator
    {
        public int Current;
        public int? Next;
        public int? Previous;
        public int Max;
        public int Count;
        public int Mid;
        public List<PaginatorLink> Layout = new();

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
        ///    << 0 ..... 34 35 36 37 38 ...... 99 >>
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
        ///    << 0 ..... 34 35 36 37 38 ...... 99 >>
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
                {
                    result.Add(new PaginatorLink(PaginatorLink.Ellipsis, null));
                }
            }

            for (var i = midStart; i <= midEnd; i++)
            {
                result.Add(new PaginatorLink(i + 1, i));
            }

            if (midEnd < Max)
            {
                if (midEnd < Max - 1)
                {
                    result.Add(new PaginatorLink(PaginatorLink.Ellipsis, null));
                }
                result.Add(new PaginatorLink(Max + 1, Max));
            }

            if (Current < Max)
            {
                result.Add(new PaginatorLink(PaginatorLink.Next, Current + 1));
                Next = Current + 1;
            }

            Layout = result;
        }

        public HString Render()
        {
            var result = new StringBuilder();
            result.AppendLine("<ul class=\"pagination\">");

            foreach (var link in Layout)
            {
                if (link.Display == PaginatorLink.Previous)
                {
                    result.AppendLine($"<li class=\"page-item\"><a class=\"page-link\" href=\"?p={link.Page}\">Previous</a></li>");
                }
                else if (link.Display == PaginatorLink.Next)
                {
                    result.AppendLine($"<li class=\"page-item\"><a class=\"page-link\" href=\"?p={link.Page}\">Next</a></li>");
                }
                else if (link.Display == PaginatorLink.Ellipsis)
                {
                    result.AppendLine("<li class=\"page-item\"><span>...</span></li>");
                }
                else
                {
                    var active = link.Page == Current ? "active" : "";
                    result.AppendLine($"<li class=\"page-item {active}\"><a class=\"page-link\" href=\"?p={link.Page}\">{link.Display}</a></li>");
                }
            }
            result.AppendLine("</ul>");

            return HString.Raw(result.ToString());
        }
    }
}
