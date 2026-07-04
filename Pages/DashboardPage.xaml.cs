namespace PCDiagnosticTool.Pages;

public class CriticalPeak
{
    public required string ComponentName { get; set; }
    public required string Timestamp { get; set; }
    public required string Description { get; set; }
}

public sealed partial class DashboardPage : Page, IDisposable
{
    private readonly HardwareMonitorService _hardwareMonitor;
    private readonly DispatcherTimer _timer;
    private readonly ObservableCollection<HardwareNode> _hardwareData = [];
    private readonly ObservableCollection<CriticalPeak> _criticalPeaks = [];

    public DashboardPage()
    {
        InitializeComponent();

        _hardwareMonitor = new HardwareMonitorService();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _timer.Tick += Timer_Tick;

        Loaded += DashboardPage_Loaded;
        Unloaded += DashboardPage_Unloaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoggerService.Log("Открыта вкладка Дашборд");
        HardwareList.ItemsSource = _hardwareData;
        CriticalPeaksList.ItemsSource = _criticalPeaks;
        _timer.Start();
    }

    private void DashboardPage_Unloaded(object sender, RoutedEventArgs e) => _timer.Stop();

    private void Timer_Tick(object? sender, object e)
    {
        var freshData = _hardwareMonitor.GetHardwareData();
        foreach (var hwNode in freshData)
        {
            var existingHw = _hardwareData.FirstOrDefault(x => x.Name == hwNode.Name);
            if (existingHw == null)
            {
                existingHw = new HardwareNode { Name = hwNode.Name, HardwareType = hwNode.HardwareType };
                _hardwareData.Add(existingHw);
            }

            foreach (var freshSensor in hwNode.Sensors)
            {
                var existingSensor = existingHw.Sensors.FirstOrDefault(s => s.Name == freshSensor.Name && s.Type == freshSensor.Type);
                if (existingSensor == null)
                {
                    existingSensor = new SensorNode { Name = freshSensor.Name, Type = freshSensor.Type };
                    existingHw.Sensors.Add(existingSensor);
                }

                existingSensor.Value = freshSensor.Value;
                existingSensor.AddHistoryRecord(freshSensor.Value ?? 0.0);

                CheckForCriticalPeak(existingHw.Name, existingSensor);
            }
        }
    }

    private void CheckForCriticalPeak(string hardwareName, SensorNode sensor)
    {
        var isCritical = false;
        var description = "";

        if (sensor.Type == "Temperature" && sensor.Value > 85.0f)
        {
            isCritical = true;
            description = $"Перегрев! Температура {sensor.Name} достигла {sensor.FormattedValue}";
        }
        else if (sensor.Type == "Load" && sensor.Value > 95.0f)
        {
            isCritical = true;
            description = $"Критическая нагрузка! {sensor.Name} загружен на {sensor.FormattedValue}";
        }

        if (isCritical)
        {
            var peak = new CriticalPeak
            {
                ComponentName = $"{hardwareName} - {sensor.Name}",
                Timestamp = DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                Description = description
            };

            if (_criticalPeaks.Count == 0 || _criticalPeaks.First().ComponentName != peak.ComponentName)
            {
                _criticalPeaks.Insert(0, peak);
                LoggerService.LogPeak(peak.ComponentName, peak.Description);
                TxtNoPeaks.Visibility = Visibility.Collapsed;
                if (_criticalPeaks.Count > 50)
                {
                    _criticalPeaks.RemoveAt(_criticalPeaks.Count - 1);
                }
            }
        }
    }

    public void Dispose() => _hardwareMonitor?.Dispose();
}
