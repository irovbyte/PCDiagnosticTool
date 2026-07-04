namespace PCDiagnosticTool;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        NavView.SelectedItem = NavView.MenuItems[0];
        NavigateTo("Dashboard");
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            NavigateTo(item.Tag?.ToString());
        }
    }

    private void NavigateTo(string? tag)
    {
        var pageType = tag switch
        {
            "Dashboard" => typeof(Pages.DashboardPage),
            "Issues" => typeof(Pages.IssuesPage),
            "Logs" => typeof(Pages.LogsPage),
            _ => null
        };

        if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
        {
            _ = ContentFrame.Navigate(pageType);
        }
    }
}
