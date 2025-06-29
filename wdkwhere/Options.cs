﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using CommandLine;

using Microsoft.Win32;

namespace Nefarius.Tools.WDKWhere;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum SubDirectory
{
    Bin,
    Build,
    CoreSystem,
    CrossCertificates,
    Include,
    Lib,
    Licenses,
    Manifests,
    Tools
}

/// <summary>
///     Common options shared across all commands.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
internal abstract class GlobalOptions
{
    [Option("build", HelpText = "Looks for a specific WDK build version (e.g. \"10.0.22621.0\").")]
    public Version? Build { get; set; } = null;

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

            Version? latestVersion = Build is null
                ? versions.First()
                : versions.SingleOrDefault(v => v == Build);

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

/// <summary>
///     The default verb that just prints the absolute installation path on success.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("query", true, HelpText = "Query the WDK path.")]
internal sealed class QueryOptions : GlobalOptions
{
}

/// <summary>
///     Runs an application within the WDK directory and optional supplied arguments.
/// </summary>
[Verb("run", HelpText = "Run the program with the specified filename and arguments.")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class RunOptions : GlobalOptions
{
    /// <summary>
    ///     The relative program filename including extension.
    /// </summary>
    [Value(0, MetaName = "filename", HelpText = "The filename to process.", Required = true)]
    public required string Filename { get; set; }
}

/// <summary>
///     Opens the WDK path in explorer.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("open", HelpText = "Opens the WDK path in explorer.")]
internal sealed class OpenOptions : GlobalOptions
{
}