namespace PCDiagnosticTool.Pages;

public sealed partial class IssuesPage : Page, IDisposable
{
    private readonly RepairService _repairService;

    public IssuesPage()
    {
        InitializeComponent();

        _repairService = new RepairService();
        _repairService.ProgressChanged += OnRepairProgressChanged;
        Loaded += IssuesPage_LoadedAsync;
    }

    private EventLogWatcher? _logWatcher;

    private async void IssuesPage_LoadedAsync(object sender, RoutedEventArgs e)
    {
        LoggerService.Log("Открыта вкладка Журнал и Починка");

        if (CrashList.ItemsSource == null)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            CrashList.Visibility = Visibility.Collapsed;

            var issues = await EventLogScanner.GetSystemIssuesAsync(24);
            CrashList.ItemsSource = issues;

            LoadingPanel.Visibility = Visibility.Collapsed;
            CrashList.Visibility = Visibility.Visible;
            StartRealTimeMonitoring();
        }
    }

    private void StartRealTimeMonitoring()
    {
        try
        {
            var query = new EventLogQuery("System", PathType.LogName, "*[System[(Level=2 or Level=3)]]");
            _logWatcher = new EventLogWatcher(query);
            _logWatcher.EventRecordWritten += OnNewEventRecord;
            _logWatcher.Enabled = true;
        }
        catch (Exception ex)
        {
            LoggerService.Log($"Ошибка подписки на журнал: {ex.Message}");
        }
    }

    private void OnNewEventRecord(object? sender, EventRecordWrittenEventArgs e)
    {
        if (e.EventRecord != null)
        {
            var ev = e.EventRecord;
            var crashEvent = new CrashEvent
            {
                TimeCreated = ev.TimeCreated ?? DateTime.Now,
                Source = ev.ProviderName,
                Id = ev.Id,
                Level = ev.Level,
                Description = ev.FormatDescription() ?? "Описание недоступно"
            };

            _ = DispatcherQueue.TryEnqueue(() =>
            {
                if (CrashList.ItemsSource is ObservableCollection<CrashEvent> currentIssues)
                {
                    currentIssues.Insert(0, crashEvent);
                    LogResult($"[НОВОЕ СОБЫТИЕ] {crashEvent.LevelName}: {crashEvent.Source} (ID: {crashEvent.Id})");
                }
            });
        }
    }

    private void OnRepairProgressChanged(object? sender, string e)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            TxtRepairLog.Text += $"[{DateTime.Now:HH:mm:ss}] {e}\n";
            TxtRepairLog.SelectionStart = TxtRepairLog.Text.Length;
        });
        LoggerService.Log($"[REPAIR] {e}");
    }

    private async void BtnAutoRepair_ClickAsync(object sender, RoutedEventArgs e)
    {
        SetAutoRepairState(true);
        LogResult("--- Запуск умного анализа системы ---");
        LogResult("Сбор информации о всех системных ошибках и предупреждениях... Это может занять несколько секунд.");

        var issues = await EventLogScanner.GetSystemIssuesAsync(24);
        var errors = issues.Count(i => i.Level == 2);
        var warnings = issues.Count(i => i.Level == 3);

        if (issues.Count > 0)
        {
            LogResult($"Найдено {errors} ошибок и {warnings} предупреждений за последние 24 часа. Начинаю восстановление...");
            _ = await _repairService.RunSystemFileCheckerAsync();
            _ = await _repairService.RunDismRestoreHealthAsync();
            LogResult("--- Умное восстановление успешно завершено ---");
            LogResult("Очистка системного журнала для старта с чистого листа...");
            await EventLogScanner.ClearSystemLogsAsync();
            if (CrashList.ItemsSource is ObservableCollection<CrashEvent> currentIssues)
            {
                currentIssues.Clear();
            }
            LogResult("Системный журнал успешно очищен! Теперь система девственно чиста.");
        }
        else
        {
            LogResult("Система девственно чиста! Ошибок и предупреждений нет. Это ИМБА! 🔥 Починка не требуется.");
        }

        SetAutoRepairState(false);
    }

    private async void BtnFixSfc_ClickAsync(object sender, RoutedEventArgs e)
    {
        SetManualRepairState(true);
        var result = await _repairService.RunSystemFileCheckerAsync();
        LogResult(result);
        SetManualRepairState(false);
    }

    private async void BtnFixDism_ClickAsync(object sender, RoutedEventArgs e)
    {
        SetManualRepairState(true);
        var result = await _repairService.RunDismRestoreHealthAsync();
        LogResult(result);
        SetManualRepairState(false);
    }

    private void LogResult(string result)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            TxtRepairLog.Text += result + "\n";
            TxtRepairLog.SelectionStart = TxtRepairLog.Text.Length;
        });
        LoggerService.Log(result);
    }

    private void SetManualRepairState(bool isRunning)
    {
        BtnFixSfc.IsEnabled = !isRunning;
        BtnFixDism.IsEnabled = !isRunning;
        BtnAutoRepair.IsEnabled = !isRunning;
        ManualRepairProgress.IsActive = isRunning;
        ManualRepairProgress.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetAutoRepairState(bool isRunning)
    {
        BtnFixSfc.IsEnabled = !isRunning;
        BtnFixDism.IsEnabled = !isRunning;
        BtnAutoRepair.IsEnabled = !isRunning;
        AutoRepairProgress.IsActive = isRunning;
        AutoRepairProgress.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
    }

    public void Dispose()
    {
        if (_logWatcher != null)
        {
            _logWatcher.Enabled = false;
            _logWatcher.Dispose();
            _logWatcher = null;
        }
    }
}
