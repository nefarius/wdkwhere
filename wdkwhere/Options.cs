using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using CommandLine;

namespace Nefarius.Tools.WDKWhere;

internal enum SubDirectory
{
    Bin,
    Tools
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class Options
{
    [Option("version", HelpText = "Looks for a specific WDK version (e.g. \"10.0.22621.0\").")]
    public Version? Version { get; set; }
    
    [Option("subdir", HelpText = "The subdirectory within the SDK root.")]
    public SubDirectory SubDirectory { get; set; } = SubDirectory.Bin;

    [Option("arch", HelpText = "The architecture within the subdirectory.")]
    public Architecture Architecture { get; set; } = RuntimeInformation.OSArchitecture;
}