using System.Diagnostics;

if (args.Length == 0)
{
    Console.Error.WriteLine("Error: No executable specified.");
    return 1;
}

try
{
    string targetExe = args[0].Trim('"');
    string executableName = Path.GetFileName(targetExe);
    if (!string.Equals(executableName, "Code.exe", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine($"Error: This launcher only supports executing Code.exe for VisualStudioCodeForCommandPalette extension. Attempted to run: {executableName}");
        return 1;
    }
    string arguments = args.Length > 1 ? string.Join(" ", args, 1, args.Length - 1) : "";
    var startInfo = new ProcessStartInfo
    {
        FileName = targetExe,
        Arguments = arguments,
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden,
        CreateNoWindow = true
    };

    Process.Start(startInfo);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to launch process: {ex}");
    return 1;
}
