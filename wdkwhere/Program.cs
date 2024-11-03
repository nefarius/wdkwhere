using CliWrap;

using CommandLine;

using Nefarius.Tools.WDKWhere;

Parser parser = new(with => with.HelpWriter = Console.Out);

ParserResult<object>? parserResult = parser.ParseArguments<QueryOptions, RunOptions, OpenOptions>(args);

try
{
    // First check global options
    await parserResult
        .WithParsed<QueryOptions>(opts =>
        {
            Console.WriteLine(opts.AbsolutePath);
        })
        .WithParsedAsync<RunOptions>(async opts =>
        {
            if (string.IsNullOrEmpty(opts.AbsolutePath))
            {
                throw new InvalidOperationException("Missing absolute path.");
            }

            string commandPath = Path.Combine(opts.AbsolutePath, opts.Filename);

            if (!Path.HasExtension(commandPath))
            {
                commandPath = $"{commandPath}.exe";
            }

            if (!File.Exists(commandPath))
            {
                Console.Error.WriteLine($"Path {commandPath} for command not found.");
                return;
            }

            await using Stream stdOut = Console.OpenStandardOutput();
            await using Stream stdErr = Console.OpenStandardError();

            Command cmd = Cli.Wrap(commandPath)
                              .WithValidation(CommandResultValidation.None)
                              .WithWorkingDirectory(Directory.GetCurrentDirectory())
                          | (stdOut, stdErr);

            if (opts.Arguments is not null)
            {
                cmd = cmd.WithArguments(opts.Arguments);
            }

            CommandResult result = await cmd.ExecuteAsync();

            Environment.Exit(result.ExitCode);
        });

    await parserResult.WithParsedAsync<OpenOptions>(async options =>
    {
        if (string.IsNullOrEmpty(options.AbsolutePath))
        {
            throw new InvalidOperationException("Missing absolute path.");
        }

        await Cli.Wrap("explorer")
            .WithValidation(CommandResultValidation.None)
            .WithArguments(options.AbsolutePath)
            .ExecuteAsync();
    });
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}