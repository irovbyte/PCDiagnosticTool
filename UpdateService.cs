namespace PCDiagnosticTool;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
}

public class UpdateService
{
    private const string RepoOwner = "irovbyte";
    private const string RepoName = "PCDiagnosticTool";
    private const string GitHubApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
    private static readonly HttpClient t_httpClient = new();

    static UpdateService() => t_httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PCDiagnosticTool", "1.0"));

    public static async Task<(bool IsUpdateAvailable, string LatestVersion, string ReleaseUrl)> CheckForUpdatesAsync()
    {
        try
        {
            var response = await t_httpClient.GetStringAsync(GitHubApiUrl);
            var release = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (release != null && !string.IsNullOrEmpty(release.TagName))
            {
                var latestVersionStr = release.TagName.TrimStart('v');
                if (Version.TryParse(latestVersionStr, out var latestVersion))
                {

                    var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);

                    if (latestVersion > currentVersion)
                    {
                        return (true, release.TagName, release.HtmlUrl);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to check for updates: {ex.Message}");
        }

        return (false, string.Empty, string.Empty);
    }
}
