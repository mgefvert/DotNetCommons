using System;

namespace DotNetCommons.Net.Cache
{
    public interface IWebCache
    {
        void Clear();
        void Clean(TimeSpan age);
        bool Exists(string uri);
        CommonWebResult Fetch(string uri);
        bool Remove(string uri);
        void Store(string uri, CommonWebResult result);
        bool TryFetch(string uri, out CommonWebResult result);
    }
}
