# DotNetCommons.Blazor controls

The components are in `DotNetCommons.Blazor.Forms` and `DotNetCommons.Blazor.Components`. Add those namespaces to `_Imports.razor`, then
use normal two-way binding:

```razor
<AppInputCurrency Title="Budget" Description="Monthly amount" Placeholder="0.00"
                  Step="0.01m" @bind-Value="budget" />
<AppInputDropDown TValue="string" Title="Team" Placeholder="Choose a team"
                  Options="teams" @bind-Value="team" />
<AppInputCheckBoxes TValue="string" Title="Interests" Placeholder="Choose any"
                    Options="interests" @bind-Value="selectedInterests" />

@code {
    private decimal? budget;
    private string team = "";
    private IReadOnlyList<string> selectedInterests = [];
    private IReadOnlyList<AppOption<string>> teams =
    [
        new("design", "Design"),
        new("engineering", "Engineering")
    ];
    private IReadOnlyList<AppOption<string>> interests =
    [
        new("design", "Design"),
        new("code", "Code")
    ];
}
```

Text inputs and memo use `string?`; number and currency use `decimal?`; date and date-time use `DateTime?`; time uses `TimeOnly?`;
range uses `decimal`; a single checkbox uses `bool`. Drop-down, radio buttons, and multiple checkboxes use `AppOption<TValue>` and 
preserve the option value type. Multiple checkboxes bind to `IReadOnlyList<TValue>` and emit a new list after each change. 
Inputs support `Title`, `Description`, `Placeholder`, `Mode`, `Id`, `Disabled`, `Required`, and standard HTML attributes. 
`ContainerCss` applies to the outer field, so a grid span such as `md:col-span-2` works directly on a control.

`Mode="AppControlMode.Compact"` reduces padding and spacing. CSS parameters such as `ContainerCss`, `LabelCss`, `DescriptionCss`, 
`InputCss`, `ChoiceCss`, `CheckableCss`, and `ButtonCss` add classes to the relevant element. `CssClassOverrides` combines these with
defaults and mode or color classes from `AppTheme`. `AppButton` renders its child content, so labels, icons, and other markup can 
be composed directly:

```razor
<AppButton OnClick="ShowClicked"><AppIcon Name="cog-8-tooth" /> Click here!</AppButton>
```

`AppTheme` provides project-wide defaults through static properties. Set them once during application startup, before components 
render. The settings apply to the whole process; use a control's CSS parameters for local changes. The host's Tailwind build must 
include any classes you assign in code.

```csharp
AppTheme.PrimaryButton = "bg-indigo-600 text-white hover:bg-indigo-700";
AppTheme.StandardControl = "rounded-md px-3 py-2 text-sm";
```

The host's Tailwind build must scan the library and any project-defined theme classes. The demo includes the library with 
`@source` in `Styles/app.css`.

`AppIconProvider` loads all SVG files in `wwwroot/icons` during startup and makes each filename (without `.svg`) available as an 
icon name. The demo registers it as a singleton in `Program.cs`; register it the same way in a server app. The directory should contain 
trusted application assets because the SVG markup is rendered directly.
