using DotNetCommons.Commands;

namespace commons.Commands;

[CommandAction(["import", "geo"], "Import all geographic data", [])]
public class ImportGeoCommand : CommandAction<ConnectionArgs>
{
    public override int Execute()
    {
        Registry.Schedule<ImportAirportsCommand,  ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);
        Registry.Schedule<ImportAreaCodesCommand, ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);
        Registry.Schedule<ImportCountriesCommand, ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);
        Registry.Schedule<ImportZipCommand,       ConnectionArgs>(CommandActionRegistry.MediumPriority, true, Args);

        return 0;
    }
}