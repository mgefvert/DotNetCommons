using DotNetCommons.Commands;

namespace commons.Commands;

[CommandAction(["import", "geo", "all"], "Import all geographic data", [])]
public class ImportGeoAllCommand : CommandAction<ConnectionArgs>
{
    public override int Execute()
    {
        Registry.Schedule<ImportGeoAirportsCommand,  ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);
        Registry.Schedule<ImportGeoAreaCodesCommand, ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);
        Registry.Schedule<ImportGeoCountriesCommand, ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);
        Registry.Schedule<ImportGeoZipCommand,       ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);

        return 0;
    }
}