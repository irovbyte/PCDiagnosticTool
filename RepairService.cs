namespace PCDiagnosticTool;

public class RepairService
{
    public event EventHandler<string>? ProgressChanged;
    public async Task<string> RunSystemFileCheckerAsync()
    {
        ProgressChanged?.Invoke(this, "Запуск сканирования SFC (System File Checker)... Это может занять несколько минут.");
        var result = await RunCommandAsync("sfc", "/scannow");
        ProgressChanged?.Invoke(this, "SFC завершено.");
        return result;
    }
    public async Task<string> RunDismRestoreHealthAsync()
    {
        ProgressChanged?.Invoke(this, "Запуск DISM RestoreHealth... Это может занять несколько минут.");
        var result = await RunCommandAsync("DISM", "/Online /Cleanup-Image /RestoreHealth");
        ProgressChanged?.Invoke(this, "DISM завершено.");
        return result;
    }
    private string? _lastOutputLine;
    private Task<string> RunCommandAsync(string fileName, string arguments)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var tcs = new TaskCompletionSource<string>();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.GetEncoding(866),
                StandardErrorEncoding = System.Text.Encoding.GetEncoding(866),
            },
            EnableRaisingEvents = true
        };
        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data == null)
            {
                return;
            }
            var cleanStr = new string([.. args.Data.Where(c => !char.IsControl(c) || c == '\n')]).Trim();
            if (!string.IsNullOrWhiteSpace(cleanStr) && cleanStr != _lastOutputLine)
            {
                _lastOutputLine = cleanStr;
                ProgressChanged?.Invoke(this, cleanStr);
            }
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data == null)
            {
                return;
            }
            var cleanStr = new string([.. args.Data.Where(c => !char.IsControl(c) || c == '\n')]).Trim();
            if (!string.IsNullOrWhiteSpace(cleanStr))
            {
                ProgressChanged?.Invoke(this, "ОШИБКА: " + cleanStr);
            }
        };
        process.Exited += (sender, args) =>
        {
            tcs.SetResult($"Процесс завершен (Код: {process.ExitCode})");
            process.Dispose();
        };
        try
        {
            _ = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }
        return tcs.Task;
    }
}
