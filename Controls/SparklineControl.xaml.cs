namespace PCDiagnosticTool.Controls;

public sealed partial class SparklineControl : UserControl
{
    public SparklineControl() => InitializeComponent();

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(ObservableCollection<double>), typeof(SparklineControl), new PropertyMetadata(null, OnDataChanged));

    public ObservableCollection<double> Data
    {
        get => (ObservableCollection<double>)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SparklineControl control)
        {
            if (e.OldValue is ObservableCollection<double> oldData)
            {
                oldData.CollectionChanged -= control.Data_CollectionChanged;
            }
            if (e.NewValue is ObservableCollection<double> newData)
            {
                newData.CollectionChanged += control.Data_CollectionChanged;
            }
            control.Redraw();
        }
    }

    private void Data_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Redraw();

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => Redraw();

    private void Redraw()
    {
        if (Data == null || Data.Count < 2 || ActualWidth == 0 || ActualHeight == 0)
        {
            LinePath.Points.Clear();
            AreaPath.Data = null;
            return;
        }

        var width = ActualWidth;
        var height = ActualHeight;
        var maxVal = 100.0;
        var minVal = 0.0;

        var points = new PointCollection();
        var count = Data.Count;
        var stepX = width / (count - 1);

        for (var i = 0; i < count; i++)
        {
            var val = Data[i];
            var normalizedY = (val - minVal) / (maxVal - minVal);
            var y = height - (normalizedY * height);
            var x = i * stepX;
            points.Add(new Point(x, y));
        }

        LinePath.Points = points;
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(0, height) };
        foreach (var pt in points)
        {
            figure.Segments.Add(new LineSegment { Point = pt });
        }
        figure.Segments.Add(new LineSegment { Point = new Point(width, height) });
        figure.IsClosed = true;

        geometry.Figures.Add(figure);
        AreaPath.Data = geometry;
    }
}
