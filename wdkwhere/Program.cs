using System.Runtime.InteropServices;

using CliWrap;

using CommandLine;

using Microsoft.Win32;

using Nefarius.Tools.WDKWhere;

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync(async o =>
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Error.WriteLine("This tool is only useful on Windows.");
            return;
        }

        RegistryKey? installedRoots =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Kits\Installed Roots");

        if (installedRoots is null)
        {
            Console.Error.WriteLine("No installed root registry key found.");
            return;
        }

        string? kitsRoot10 = installedRoots.GetValue("KitsRoot10") as string;

        if (string.IsNullOrEmpty(kitsRoot10))
        {
            Console.Error.WriteLine("KitsRoot10 registry value not found.");
            return;
        }

        List<Version> versions = installedRoots.GetSubKeyNames()
            .Select(name => Version.TryParse(name, out Version? version) ? version : null)
            .Where(v => v is not null)
            .OrderByDescending(v => v)
            .ToList()!;

        if (versions.Count == 0)
        {
            Console.Error.WriteLine("No versions registry key found.");
            return;
        }

        Version? latestVersion = o.Version is null
            ? versions.First()
            : versions.SingleOrDefault(v => v == o.Version);

        if (latestVersion is null)
        {
            Console.Error.WriteLine("No matching version found.");
            return;
        }

        string absolutePath = Path.Combine(kitsRoot10, o.SubDirectory.ToString(), latestVersion.ToString(),
            o.Architecture.ToString());

        if (!Directory.Exists(absolutePath))
        {
            Console.Error.WriteLine($"Path {absolutePath} not found.");
            return;
        }

        // run a WDK application and mirror stdout, stderr and exit code
        if (!string.IsNullOrEmpty(o.RunCommand))
        {
            string commandPath = Path.Combine(absolutePath, o.RunCommand);

            if (!File.Exists(commandPath))
            {
                Console.Error.WriteLine($"Path {commandPath} for command not found.");
                return;
            }

            await using Stream stdOut = Console.OpenStandardOutput();
            await using Stream stdErr = Console.OpenStandardError();

            Command cmd = Cli.Wrap(commandPath)
                              .WithValidation(CommandResultValidation.None)
                              .WithWorkingDirectory(absolutePath)
                          | (stdOut, stdErr);

            if (o.RunCommandArguments is not null)
            {
                cmd.WithArguments(o.RunCommandArguments);
            }

            CommandResult result = await cmd.ExecuteAsync();

            Environment.ExitCode = result.ExitCode;
            return;
        }

        // default is to output the computed path
        Console.WriteLine(absolutePath);
    });