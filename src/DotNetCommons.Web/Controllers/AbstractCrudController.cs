using DotNetCommons.EF;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCommons.Web.Controllers;

[ApiController]
public abstract class AbstractCrudController<TDataKey, TDataObject, TListQuery, TCrudService> : Controller
    where TDataKey : notnull
    where TDataObject : class, new()
    where TListQuery : class
    where TCrudService : ICrudService<TDataKey, TDataObject, TListQuery>
{
    protected TCrudService Service { get; }
    protected ICrudLogOperation<TDataObject, TDataKey>? Logger { get; }

    protected AbstractCrudController(TCrudService crudService, ICrudLogOperation<TDataObject, TDataKey>? logger = null)
    {
        Service = crudService;
        Logger  = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<TDataKey[]>> Create([FromBody] TDataObject[] items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        try
        {
            var result = await Service.Create(items, cancellationToken);

            if (Logger != null)
                await Logger.CompletedRequest(nameof(Create), items);
            return result;
        }
        catch (Exception e)
        {
            if (Logger != null)
                await Logger.AbortedRequest(nameof(Create), items, e);
            throw;
        }
    }

    [HttpGet("get")]
    public async Task<ActionResult<TDataObject[]>> Get([FromQuery] TDataKey[] ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        try
        {
            var result = await Service.Get(ids, cancellationToken);

            if (Logger != null)
                await Logger.CompletedRequest(nameof(Get), ids);
            return result;
        }
        catch (Exception e)
        {
            if (Logger != null)
                await Logger.AbortedRequest(nameof(Get), ids, e);
            throw;
        }
    }

    [HttpGet("get/{id}")]
    public async Task<ActionResult<TDataObject>> Get(TDataKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var result = await Service.Get(id, cancellationToken);

            if (Logger != null)
                await Logger.CompletedRequest(nameof(Get), [id]);
            return result;
        }
        catch (Exception e)
        {
            if (Logger != null)
                await Logger.AbortedRequest(nameof(Get), [id], e);
            throw;
        }
    }

    [HttpGet("list")]
    public async Task<ActionResult<TDataObject[]>> List([FromQuery] TListQuery? query, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await Service.List(query, cancellationToken);

            if (Logger != null)
                await Logger.CompletedRequest(nameof(List), Request.QueryString.ToString(), result.Length);
            return result;
        }
        catch (Exception e)
        {
            if (Logger != null)
                await Logger.AbortedRequest(nameof(List), Request.QueryString.ToString(), e);
            throw;
        }
    }

    [HttpPost("update")]
    public async Task<ActionResult<TDataKey[]>> Update([FromBody] TDataObject[] items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        try
        {
            var result = await Service.Update(items, cancellationToken);

            if (Logger != null)
                await Logger.CompletedRequest(nameof(Update), items);
            return result;
        }
        catch (Exception e)
        {
            if (Logger != null)
                await Logger.AbortedRequest(nameof(Update), items, e);
            throw;
        }
    }

    [HttpPost("delete")]
    public async Task<ActionResult<TDataKey[]>> Delete([FromForm] TDataKey[] ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        try
        {
            var result = await Service.Delete(ids, cancellationToken);

            if (Logger != null)
                await Logger.CompletedRequest(nameof(Delete), ids);
            return result;
        }
        catch (Exception e)
        {
            if (Logger != null)
                await Logger.AbortedRequest(nameof(Delete), ids, e);
            throw;
        }
    }
}