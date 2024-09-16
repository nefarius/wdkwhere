using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using CommandLine;

namespace Nefarius.Tools.WDKWhere;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum SubDirectory
{
    Bin,
    Tools
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class Options
{
    [Option("version", HelpText = "Looks for a specific WDK version (e.g. \"10.0.22621.0\").")]
    public Version? Version { get; set; } = null;

    [Option("subdir", HelpText = "The subdirectory within the SDK root.")]
    public SubDirectory SubDirectory { get; set; } = SubDirectory.Bin;

    [Option("arch", HelpText = "The architecture within the subdirectory.")]
    public Architecture Architecture { get; set; } = RuntimeInformation.OSArchitecture;

    [Option("run", HelpText = "Runs an executable found in the computed path.")]
    public string? RunCommand { get; set; }

    [Option("arg", HelpText = "Launch arguments for the executable.")]
    public IEnumerable<string>? RunCommandArguments { get; set; }
}