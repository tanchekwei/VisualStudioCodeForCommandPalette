using System.Diagnostics;

if (args.Length == 0)
{
    Console.Error.WriteLine("Error: No executable specified.");
    return 1;
}

try
{
    string targetExe = args[0].Trim('"');
    string arguments = args.Length > 1
        ? string.Join(" ", args, 1, args.Length - 1)
        : string.Empty;

    var startInfo = new ProcessStartInfo
    {
        FileName = targetExe,
        Arguments = arguments,
        UseShellExecute = false,
    };

    startInfo.Environment["PATH"] = GetUpdatedPath();

    Process.Start(startInfo);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to launch process: {ex}");
    return 1;
}

static string GetUpdatedPath()
{
    var machinePath = Environment.GetEnvironmentVariable(
        "PATH",
        EnvironmentVariableTarget.Machine);

    var userPath = Environment.GetEnvironmentVariable(
        "PATH",
        EnvironmentVariableTarget.User);

    if (string.IsNullOrWhiteSpace(machinePath))
    {
        return userPath ?? string.Empty;
    }

    if (string.IsNullOrWhiteSpace(userPath))
    {
        return machinePath;
    }

    return machinePath + Path.PathSeparator + userPath;
}
