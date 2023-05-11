using System;

namespace DotNetCommons.Web;

[AttributeUsage(AttributeTargets.Method)]
public class SiteMapAttribute : Attribute
{
    public decimal Priority { get; }

    public SiteMapAttribute(decimal priority)
    {
        Priority = priority;
    }
}