using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCommons.Web;

public static class ControllerExtensions
{
    /// <summary>
    /// Helper method for rendering a custom view to a string.
    /// </summary>
    /// <param name="controller">The current controller.</param>
    /// <param name="viewName">The name of the view to render.</param>
    /// <param name="model">Which model to send into the view.</param>
    /// <param name="partial">Is it a partial view?</param>
    /// <returns>The rendered view as a string.</returns>
    public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
    {
        if (string.IsNullOrEmpty(viewName))
            viewName = controller.ControllerContext.ActionDescriptor.ActionName;

        controller.ViewData.Model = model;

        var viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
        var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);
        if (viewResult.Success == false)
            return $"A view with the name {viewName} could not be found";

        await using var writer = new StringWriter();

        var viewContext = new ViewContext(
            controller.ControllerContext,
            viewResult.View,
            controller.ViewData,
            controller.TempData,
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);

        return writer.GetStringBuilder().ToString();
    }
}