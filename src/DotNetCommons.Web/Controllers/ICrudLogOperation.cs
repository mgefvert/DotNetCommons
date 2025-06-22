namespace DotNetCommons.Web.Controllers;

public interface ICrudLogOperation<in TDataObject, in TDataKey>
    where TDataKey : notnull
    where TDataObject : class, new()
{
    Task AbortedRequest(string operation, TDataObject[] items, Exception? exception);
    Task AbortedRequest(string operation, TDataKey[] keys, Exception? exception);
    Task AbortedRequest(string operation, string request, Exception? exception);

    Task CompletedRequest(string operation, TDataObject[] items);
    Task CompletedRequest(string operation, TDataKey[] keys);
    Task CompletedRequest(string operation, string request, int items);
}