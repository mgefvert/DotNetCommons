using System;

namespace DotNetCommons.Web;

public class SitemapAttribute : Attribute
{
    public float Priority { get; }

    public SitemapAttribute(float priority = 0.8f)
    {
        Priority = priority;
    }
}