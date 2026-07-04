namespace PCDiagnosticTool;

public partial class SensorNode : INotifyPropertyChanged
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public float? Value
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedValue));
            }
        }
    }
    public ObservableCollection<double> History { get; } = [];
    public void AddHistoryRecord(double val)
    {
        History.Add(val);
        if (History.Count > 60)
        {
            History.RemoveAt(0);
        }
    }
    public string FormattedValue => !Value.HasValue
                ? "--"
                : Type switch
                {
                    "Temperature" => $"{Value.Value:F1} °C",
                    "Load" => $"{Value.Value:F1} %",
                    "Voltage" => $"{Value.Value:F3} V",
                    "Power" => $"{Value.Value:F1} W",
                    "Fan" => $"{Value.Value:F0} RPM",
                    "Clock" => $"{Value.Value:F0} MHz",
                    "Data" => $"{Value.Value:F1} GB",
                    "SmallData" => $"{Value.Value:F1} MB",
                    _ => $"{Value.Value:F1}"
                };
    public Brush ColorBrush => Type switch
    {
        "Temperature" => new SolidColorBrush(Colors.OrangeRed),
        "Load" => new SolidColorBrush(Colors.DodgerBlue),
        "Voltage" => new SolidColorBrush(Colors.Gold),
        "Power" => new SolidColorBrush(Colors.MediumPurple),
        "Fan" => new SolidColorBrush(Colors.LightSeaGreen),
        _ => new SolidColorBrush(Colors.Gray)
    };
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
public partial class HardwareNode : INotifyPropertyChanged
{
    public required string Name { get; set; }
    public required string HardwareType { get; set; }
    public ObservableCollection<SensorNode> Sensors { get; } = [];
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
public partial class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer;
    public HardwareMonitorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsStorageEnabled = true
        };
        try
        {
            _computer.Open();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open hardware monitor: {ex.Message}");
        }
    }
    public List<HardwareNode> GetHardwareData()
    {
        var dataList = new List<HardwareNode>();
        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();
            var hwNode = new HardwareNode
            {
                Name = hardware.Name,
                HardwareType = hardware.HardwareType.ToString()
            };
            foreach (var sensor in hardware.Sensors)
            {
                var sensorNode = new SensorNode
                {
                    Name = sensor.Name,
                    Type = sensor.SensorType.ToString(),
                    Value = sensor.Value
                };
                hwNode.Sensors.Add(sensorNode);
            }
            dataList.Add(hwNode);
            foreach (var sub in hardware.SubHardware)
            {
                sub.Update();
                var subNode = new HardwareNode
                {
                    Name = sub.Name,
                    HardwareType = sub.HardwareType.ToString()
                };
                foreach (var sensor in sub.Sensors)
                {
                    var sensorNode = new SensorNode
                    {
                        Name = sensor.Name,
                        Type = sensor.SensorType.ToString(),
                        Value = sensor.Value
                    };
                    subNode.Sensors.Add(sensorNode);
                }
                dataList.Add(subNode);
            }
        }
        return dataList;
    }
    public void Dispose()
    {
        _computer?.Close();
        GC.SuppressFinalize(this);
    }
}
