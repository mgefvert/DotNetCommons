

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Web.Html;

public class MenuItem
{
    public string Title { get; private set; }
    public string Link { get; private set; }
    public string? Icon { get; private set; }
    public bool Active { get; set; }
    public List<MenuItem>? Submenu { get; set; }

    public MenuItem(string title, string link)
    {
        Title = title;
        Link = link;
    }

    public MenuItem WithIcon(string icon)
    {
        Icon = icon;
        return this;
    }

    public MenuItem WithSubmenu(IReadOnlyCollection<MenuItem> menu)
    {
        Submenu = menu.ToList();
        return this;
    }
}