using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace TrimmedMagazines;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.desze.trimmedmags";
    public override string Name { get; init; } = "Trimmed Magazines";
    public override string Author { get; init; } = "desze";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "CC BY-NC 3.0";
}

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class TrimmedMagazines(
    ISptLogger<TrimmedMagazines> logger,
    DatabaseService databaseService) : IOnLoad
{
    private const string ModName = "Trimmed Magazines";
    private const int TargetHeight = 2;
    private const int MinMagCapacity = 10;
    private const int MaxMagCapacity = 50;

    public Task OnLoad()
    {
        var itemsChanged = 0;

        foreach (var item in databaseService.GetItems().Values)
        {
            if (TryTrimMagazine(item))
            {
                itemsChanged++;
            }
        }

        logger.LogWithColor(
            $"{ModName} successfully loaded!",
            LogTextColor.Cyan);

        return Task.CompletedTask;
    }

    private static bool TryTrimMagazine(TemplateItem item)
    {
        if (item.Parent != BaseClasses.MAGAZINE)
        {
            return false;
        }

        var properties = item.Properties;
        if (properties is null || properties.Width != 1 || properties.Height is not > TargetHeight)
        {
            return false;
        }

        var capacity = properties.Cartridges?
            .FirstOrDefault(cartridge => cartridge.MaxCount is not null)?
            .MaxCount;

        if (capacity is null ||
            capacity < MinMagCapacity ||
            capacity > MaxMagCapacity)
        {
            return false;
        }

        var heightDelta = properties.Height!.Value - TargetHeight;
        properties.Height = TargetHeight;

        if (properties.ExtraSizeDown is > 0)
        {
            properties.ExtraSizeDown = Math.Max(0, properties.ExtraSizeDown.Value - heightDelta);
        }

        return true;
    }
}