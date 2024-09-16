using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using CommandLine;

using Microsoft.Win32;

namespace Nefarius.Tools.WDKWhere;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum SubDirectory
{
    Bin,
    Tools
}

internal abstract class GlobalOptions
{
    [Option("version", HelpText = "Looks for a specific WDK version (e.g. \"10.0.22621.0\").")]
    public Version? Version { get; set; } = null;

    [Option("subdir", HelpText = "The subdirectory within the SDK root.")]
    public SubDirectory SubDirectory { get; set; } = SubDirectory.Bin;

    [Option("arch", HelpText = "The architecture within the subdirectory.")]
    public Architecture Architecture { get; set; } = RuntimeInformation.OSArchitecture;

    /// <summary>
    ///     The computed absolute path.
    /// </summary>
    public string? AbsolutePath
    {
        get
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new InvalidOperationException("This tool is only useful on Windows.");
            }

            RegistryKey? installedRoots =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Kits\Installed Roots");

            if (installedRoots is null)
            {
                throw new InvalidOperationException("No installed root registry key found.");
            }

            string? kitsRoot10 = installedRoots.GetValue("KitsRoot10") as string;

            if (string.IsNullOrEmpty(kitsRoot10))
            {
                throw new InvalidOperationException("KitsRoot10 registry value not found.");
            }

            List<Version> versions = installedRoots.GetSubKeyNames()
                .Select(name => Version.TryParse(name, out Version? version) ? version : null)
                .Where(v => v is not null)
                .OrderByDescending(v => v)
                .ToList()!;

            if (versions.Count == 0)
            {
                throw new InvalidOperationException("No versions registry key found.");
            }

            Version? latestVersion = Version is null
                ? versions.First()
                : versions.SingleOrDefault(v => v == Version);

            if (latestVersion is null)
            {
                throw new InvalidOperationException("No matching version found.");
            }

            string absolutePath = Path.Combine(
                kitsRoot10,
                SubDirectory.ToString(),
                latestVersion.ToString(),
                Architecture.ToString()
            );

            if (!Directory.Exists(absolutePath))
            {
                throw new InvalidOperationException($"Path {absolutePath} not found.");
            }

            return absolutePath;
        }
    }
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("query", true, HelpText = "Query the WDK path.")]
internal sealed class QueryOptions : GlobalOptions
{
}

[Verb("run", HelpText = "Run the program with the specified filename and arguments.")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class RunOptions : GlobalOptions
{
    [Value(0, MetaName = "filename", HelpText = "The filename to process.", Required = true)]
    public required string Filename { get; set; }

    [Value(1, MetaName = "arguments", HelpText = "Additional CLI arguments.", Required = false)]
    public IEnumerable<string>? Arguments { get; set; }
}