using System.Linq.Expressions;
using DotNetCommons.Base;
using DotNetCommons.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace DotNetCommons.Blazor.Forms
{
    public enum AppControlMode { Standard, Compact }

    public abstract class AppInputBase<T> : ComponentBase
    {
        [Parameter] public string? Title { get; set; }
        [Parameter] public string? Description { get; set; }
        [Parameter] public string? Placeholder { get; set; }
        [Parameter] public T Value { get; set; } = default!;
        [Parameter] public EventCallback<T> ValueChanged { get; set; }
        [Parameter] public Expression<Func<T>>? ValueExpression { get; set; }
        [Parameter] public string? Id { get; set; }
        [Parameter] public string? ContainerCss { get; set; }
        [Parameter] public string? LabelCss { get; set; }
        [Parameter] public string? DescriptionCss { get; set; }
        [Parameter] public string? InputCss { get; set; }
        [Parameter] public string? ChoiceGroupCss { get; set; }
        [Parameter] public string? ChoiceCss { get; set; }
        [Parameter] public string? CheckableCss { get; set; }
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool Required { get; set; }
        [Parameter] public AppControlMode Mode { get; set; } = AppControlMode.Standard;
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

        protected string FieldId { get; } = $"app-{Guid.NewGuid():N}";
        protected string EffectiveId => Id ?? FieldId;
        protected readonly CssClassOverrides _containerCss = new();
        protected readonly CssClassOverrides _labelCss = new();
        protected readonly CssClassOverrides _descriptionCss = new();
        protected readonly CssClassOverrides _inputCss = new();
        protected readonly CssClassOverrides _choiceGroupCss = new();
        protected readonly CssClassOverrides _choiceCss = new();
        protected readonly CssClassOverrides _checkableCss = new();

        protected override void OnParametersSet()
        {
            _containerCss.Defaults  = AppTheme.Container;
            _containerCss.Overrides = ContainerCss;
            _containerCss.State     = Mode == AppControlMode.Compact ? AppTheme.CompactContainer : AppTheme.StandardContainer;

            _labelCss.Defaults  = AppTheme.Label;
            _labelCss.Overrides = LabelCss;
            _labelCss.State     = Mode == AppControlMode.Compact ? AppTheme.CompactLabel : AppTheme.StandardLabel;

            _descriptionCss.Defaults  = AppTheme.Description;
            _descriptionCss.Overrides = DescriptionCss;

            _inputCss.Defaults  = AppTheme.Control;
            _inputCss.Overrides = InputCss;
            _inputCss.State     = Mode == AppControlMode.Compact ? AppTheme.CompactControl : AppTheme.StandardControl;

            _choiceGroupCss.Defaults  = AppTheme.ChoiceGroup;
            _choiceGroupCss.Overrides = ChoiceGroupCss;
            _choiceGroupCss.State     = Mode == AppControlMode.Compact ? AppTheme.CompactChoiceGroup : AppTheme.StandardChoiceGroup;

            _choiceCss.Defaults  = AppTheme.Choice;
            _choiceCss.Overrides = ChoiceCss;
            _choiceCss.State     = Mode == AppControlMode.Compact ? AppTheme.CompactChoice : AppTheme.StandardChoice;

            _checkableCss.Defaults  = AppTheme.Checkable;
            _checkableCss.Overrides = CheckableCss;
        }

        protected Task SetValueAsync(T value) => ValueChanged.InvokeAsync(value);
    }
}
