using System;

namespace DotNetCommons.Net
{
    public interface IWebCache
    {
        bool Exists(string uri);
        CommonWebResult Fetch(string uri);
        void Store(string uri, CommonWebResult result);
        bool TryFetch(string uri, out CommonWebResult result);
    }
}
