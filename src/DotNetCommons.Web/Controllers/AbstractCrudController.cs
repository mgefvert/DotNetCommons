using DotNetCommons.EF;
using DotNetCommons.Web.Logging;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCommons.Web.Controllers;

[ApiController]
public abstract class AbstractCrudController<TDataKey, TDataObject, TListQuery, TCrudService> : Controller
    where TDataKey : notnull
    where TDataObject : class, new()
    where TListQuery : class
    where TCrudService : ICrudService<TDataKey, TDataObject, TListQuery>
{
    protected ApiLogRequest Log { get; }
    protected TCrudService Service { get; }

    protected abstract string ObjectToString(TDataObject item);

    protected AbstractCrudController(TCrudService crudService, ApiLogRequest log)
    {
        Log = log;
        Service = crudService;
    }

    [Route("create"), HttpPost()]
    public async Task<ActionResult<TDataKey[]>> Create([FromBody] TDataObject[] items)
    {
        Log.Request($"{items.Length} items");
        var result = await Service.Create(items);
        
        Log.Data($"Created: " + string.Join(",", result));
        Log.Data(items.Select(ObjectToString));
        return result;
    }

    [HttpGet("get")]
    public async Task<ActionResult<TDataObject[]>> Get([FromQuery] TDataKey[] ids)
    {
        Log.Request(string.Join(",", ids));
        var result = await Service.Get(ids);
        
        Log.Data($"{result.Length} items");
        return result;
    }

    [HttpGet("get/{id}")]
    public async Task<ActionResult<TDataObject>> Get(TDataKey id)
    {
        Log.Request(id.ToString());
        
        var result = await Service.Get(id);
        return result;
    }

    [HttpGet("list")]
    public async Task<ActionResult<TDataObject[]>> List([FromQuery] TListQuery? query)
    {
        Log.Request(Request.QueryString);
        var result = await Service.List(query);
        
        Log.Data($"{result.Length} items");
        return result;
    }

    [HttpPost("update")]
    public async Task<ActionResult<TDataKey[]>> Update([FromBody] TDataObject[] items)
    {
        Log.Request($"{items.Length} items");
        var result = await Service.Update(items);
        
        Log.Data($"{result.Length} items updated");
        Log.Data(items.Select(ObjectToString));
        return result;
    }

    [HttpPost("delete")]
    public async Task<ActionResult<TDataKey[]>> Delete([FromForm] TDataKey[] ids)
    {
        Log.Request(string.Join(",", ids));
        var result = await Service.Delete(ids);
        
        Log.Data($"{result} items deleted");
        Log.Data(string.Join(",", result));
        return result;
    }
}