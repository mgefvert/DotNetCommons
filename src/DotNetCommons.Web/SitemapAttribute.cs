using System;

namespace DotNetCommons.Web;

[AttributeUsage(AttributeTargets.Method)]
public class SiteMapAttribute : Attribute
{
    public float Priority { get; }

    public SiteMapAttribute(float priority = 0.8f)
    {
        Priority = priority;
    }
}