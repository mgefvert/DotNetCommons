namespace DotNetCommons.EF;

public interface ICrudService<TDataKey, TDataObject, in TListQuery>
    where TListQuery : class
{
    Task<TDataKey[]> Create(TDataObject[] items);
    Task<TDataObject[]> Get(TDataKey[] ids);
    Task<TDataObject> Get(TDataKey id);
    Task<TDataObject[]> List(TListQuery? query = null);
    Task<TDataKey[]> Update(TDataObject[] items);
    Task<TDataKey[]> Delete(TDataKey[] ids);
}
