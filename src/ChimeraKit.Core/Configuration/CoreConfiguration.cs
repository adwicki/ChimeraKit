namespace ChimeraKit.Core.Configuration;

public sealed class CoreConfiguration
{
    public const string SectionName = "Core";

    public string ModuleRoot { get; set; } = string.Empty;
    public ModuleInformation[] AvailableModules { get; set; } = [];
}