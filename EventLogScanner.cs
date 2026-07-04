namespace PCDiagnosticTool;

public class CrashEvent
{
    public DateTime TimeCreated { get; set; }
    public required string Source { get; set; }
    public int Id { get; set; }
    public required string Description { get; set; }
    public byte? Level { get; set; }
    public string LevelName => Level == 2 ? "Ошибка" : Level == 3 ? "Предупреждение" : "Сбой";
    public SolidColorBrush StatusBrush => Level == 2 ? new SolidColorBrush(Color.FromArgb(255, 255, 85, 85)) :
                                          Level == 3 ? new SolidColorBrush(Color.FromArgb(255, 255, 170, 0)) :
                                          new SolidColorBrush(Colors.Gray);
}
public class EventLogScanner
{
    public static Task<ObservableCollection<CrashEvent>> GetSystemIssuesAsync(int hours = 24)
    {
        return Task.Run(() =>
        {
            var issues = new List<CrashEvent>();
            var queryStr = $"*[System[(Level=2 or Level=3) and TimeCreated[timediff(@SystemTime) <= {hours * 3600000}]]]";
            try
            {
                var query = new EventLogQuery("System", PathType.LogName, queryStr);
                using var reader = new EventLogReader(query);
                EventRecord ev;
                while ((ev = reader.ReadEvent()) != null)
                {
                    issues.Add(new CrashEvent
                    {
                        TimeCreated = ev.TimeCreated ?? DateTime.MinValue,
                        Source = ev.ProviderName,
                        Id = ev.Id,
                        Level = ev.Level,
                        Description = ev.FormatDescription() ?? "Описание недоступно"
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading event logs: {ex.Message}");
            }
            return new ObservableCollection<CrashEvent>(issues.OrderByDescending(c => c.TimeCreated));
        });
    }
    public static async Task ClearSystemLogsAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo("wevtutil", "cl System")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi)?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing event logs: {ex.Message}");
            }
        });
    }
}
