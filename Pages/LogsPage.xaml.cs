namespace PCDiagnosticTool.Pages;

public sealed partial class LogsPage : Page
{
    public LogsPage()
    {
        InitializeComponent();
        Loaded += LogsPage_Loaded;
    }

    private void LogsPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoggerService.Log("Открыта вкладка Логи");
        LoadCurrentLog();
    }

    private void LoadCurrentLog()
    {
        try
        {
            var logPath = LoggerService.GetCurrentLogFile();
            if (File.Exists(logPath))
            {
                using var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);
                TxtCurrentLog.Text = sr.ReadToEnd();
                TxtCurrentLog.SelectionStart = TxtCurrentLog.Text.Length;
            }
            else
            {
                TxtCurrentLog.Text = "Файл лога пока пуст.";
            }
        }
        catch (Exception ex)
        {
            TxtCurrentLog.Text = $"Ошибка при чтении лога: {ex.Message}";
        }
    }

    private void BtnRefreshLogs_Click(object sender, RoutedEventArgs e) => LoadCurrentLog();

    private void BtnOpenLogFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var folder = LoggerService.GetLogDirectory();
            _ = Process.Start("explorer.exe", $"\"{folder}\"");
        }
        catch (Exception ex)
        {
            LoggerService.Log($"Не удалось открыть папку логов: {ex.Message}");
        }
    }
}
