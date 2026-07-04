namespace PCDiagnosticTool;

public class LoggerService
{
    private static readonly string t_logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PCDiagnosticTool", "Logs");
    private static readonly string t_currentLogFile = Path.Combine(t_logDirectory, $"DiagnosticLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

    private static readonly string t_peaksLogFile = Path.Combine(t_logDirectory, "CriticalPeaks.txt");

    static LoggerService()
    {
        if (!Directory.Exists(t_logDirectory))
        {
            _ = Directory.CreateDirectory(t_logDirectory);
        }
    }

    public static void Log(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        File.AppendAllText(t_currentLogFile, logEntry + Environment.NewLine);
        Debug.WriteLine(logEntry);
    }

    public static void LogPeak(string component, string description)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] PEAK [{component}]: {description}";
        File.AppendAllText(t_peaksLogFile, logEntry + Environment.NewLine);
        Debug.WriteLine(logEntry);
    }

    public static string GetLogDirectory() => t_logDirectory;

    public static string GetCurrentLogFile() => t_currentLogFile;
}
