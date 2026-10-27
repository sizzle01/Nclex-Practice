using System.Globalization;
using System.Text.Json;
using Nclex.Application;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Infrastructure;

namespace Nclex.Worker;

/// <summary>
/// <c>dotnet run --project src/Nclex.Worker -- import-questions &lt;file.json&gt;</c>: loads a question
/// file, publishes its questions and exits. It lives in the worker so the solution keeps the
/// layout of architecture.md section 4 and reuses the worker's configuration.
/// </summary>
internal static class ImportQuestionsCommand
{
    public const string Name = "import-questions";

    private const int Success = 0;
    private const int InvalidContent = 1;
    private const int BadUsage = 2;

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length != 1)
        {
            await Console.Error.WriteLineAsync($"Usage: {Name} <path to questions .json>");
            return BadUsage;
        }

        // dotnet run starts the worker in its project folder, so a relative path is resolved from
        // there; the walkthrough passes "$PWD/..." for that reason.
        var path = Path.GetFullPath(args[0]);
        if (!File.Exists(path))
        {
            await Console.Error.WriteLineAsync($"File not found: {path}");
            return BadUsage;
        }

        var builder = Host.CreateApplicationBuilder();
        // EF Core logs every SQL command at Information; the summary below is all a user needs.
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Services.AddInfrastructure();
        builder.Services.AddContentImport();
        builder.Services.AddSingleton(TimeProvider.System);
        using var host = builder.Build();

        QuestionImportFile file;
        try
        {
            file = QuestionImportFile.Parse(await File.ReadAllTextAsync(path));
        }
        catch (JsonException ex)
        {
            await Console.Error.WriteLineAsync($"{path} is not a valid question file: {ex.Message}");
            return InvalidContent;
        }

        await using var scope = host.Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ImportQuestionsHandler>();
        var result = await handler.HandleAsync(file, CancellationToken.None);

        if (!result.Succeeded)
        {
            await Console.Error.WriteLineAsync($"Nothing was imported. {result.Issues.Count} problem(s) in {path}:");
            foreach (var issue in result.Issues)
            {
                await Console.Error.WriteLineAsync($"  {issue.Path}: {issue.Code} - {issue.Message}");
            }

            return InvalidContent;
        }

        Console.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Imported {file.Questions.Count} {file.Track} question(s): {result.Created} created, {result.Updated} updated, {result.Unchanged} unchanged."));
        return Success;
    }
}
