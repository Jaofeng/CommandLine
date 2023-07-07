using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.Versioning;

namespace CJF.CommandLine;

#region Public Interface : ICliHostedService
/// <summary>定義 <see cref="CliHostedService"/> 服務的介面。</summary>
[UnsupportedOSPlatform("browser")]
public interface ICliHostedService : IHostedService
{
    /// <summary>取得實際執行的 <see cref="CliCenter"/> 執行個體。</summary>
    CliCenter Current { get; }
}
#endregion

#region Public Sealed Class : CliOptions
/// <summary>提供 <see cref="CliHostedService"/> 的設定項目。</summary>
[UnsupportedOSPlatform("browser")]
public sealed class CliOptions
{
    /// <summary>設定或取得 CLI 的指令提示字串。</summary>
    public string Prompt { get; set; } = "> ";
    /// <summary>設定或取得 CLI 的指令提示字串的顏色。</summary>
    public ConsoleColor PromptColor { get; set; } = Console.ForegroundColor;
    /// <summary>設定或取得歷史指令清單的分類字串。</summary>
    public string HistoryPool { get; set; } = CliCenter.DEFAULT_POOL;
    /// <summary>設定或取得密碼輸入時的顯示字元。</summary>
    public char? PasswordChar { get; set; }
    /// <summary>設定或取得是否啟用除錯模式。</summary>
    public bool DebugMode { get; set; } = false;
    /// <summary>開始輸入前等待的時間，單位豪秒，預設為 1000 豪秒。</summary>
    public int Pause { get; set; } = 1000;
}
#endregion

#region Public Sealed Class : CliHostedService
/// <summary>以 <see cref="CliCenter"/> 類別，提供主控台 <see cref="Console"/> 的命令控制介面(Command Line Interface, CLI) 的 <see cref="IHostedService"/> 服務。</summary>
[UnsupportedOSPlatform("browser")]
public sealed class CliHostedService : ICliHostedService, IDisposable
{
    private readonly CliCenter _center;
    /// <summary>取得實際執行的 <see cref="CliCenter"/> 執行個體。</summary>
    public CliCenter Current => _center;

    /// <summary>建立 <see cref="CliHostedService"/> 服務。</summary>
    /// <param name="options">選項設定函示。</param>
    /// <exception cref="HostAbortedException"></exception>
    public CliHostedService(Action<CliOptions> options)
    {
        var _opts = new CliOptions();
        options.Invoke(_opts);
        _center = new CliCenter(_opts);
    }
    /// <summary>開始執行 <see cref="CliHostedService"/> 服務。</summary>
    public Task StartAsync(CancellationToken cancellationToken) => _center.StartAsync(cancellationToken);
    /// <summary>停止執行 <see cref="CliHostedService"/> 服務。</summary>
    public Task StopAsync(CancellationToken cancellationToken) => _center.StopAsync(cancellationToken);
    /// <summary>釋放 <see cref="CliHostedService"/> 所使用的資源。</summary>
    public void Dispose() { }
}
#endregion

#region Public Static Class : CliHostedServiceExtensions
/// <summary>提供 <see cref="CliHostedService"/> 服務的擴充方法。</summary>
public static class CliHostedServiceExtensions
{
    #region Public Static Method : IHostBuilder UseCommandLine(this IHostBuilder hostBuilder)
    /// <summary>使用預設的 <see cref="CliOptions"/> 來建立 <see cref="CliHostedService"/> 服務，提供程式內命令列(<see href="https://en.wikipedia.org/wiki/Command-line_interface">Command Line Interface</see>)功能。</summary>
    /// <param name="hostBuilder">由 <see cref="Host"/> 建立出來的 <see cref="IHostBuilder"/> 執行個體。</param>
    /// <returns>建立 <see cref="CliHostedService"/> 服務完成後的 <see cref="IHostBuilder"/> 執行個體。</returns>
    public static IHostBuilder UseCommandLine(this IHostBuilder hostBuilder)
    {
        return hostBuilder.CreateCliHostedService(null, null);
    }
    #endregion

    #region Public Static Method : IHostBuilder UseCommandLine(this IHostBuilder hostBuilder, Action<CliOptions> options)
    /// <summary>使用 <see cref="CliHostedService"/> 服務，提供程式內命令列(<see href="https://en.wikipedia.org/wiki/Command-line_interface">Command Line Interface</see>)功能。</summary>
    /// <param name="hostBuilder">由 <see cref="Host"/> 建立出來的 <see cref="IHostBuilder"/> 執行個體。</param>
    /// <param name="options">設定 <see cref="CliOptions"/> 選項的執行函示。</param>
    /// <returns>建立 <see cref="CliHostedService"/> 服務完成後的 <see cref="IHostBuilder"/> 執行個體。</returns>
    public static IHostBuilder UseCommandLine(this IHostBuilder hostBuilder, Action<CliOptions> options)
    {
        return hostBuilder.CreateCliHostedService(options);
    }
    #endregion

    #region Public Static Method : IHostBuilder UseCommandLine(this IHostBuilder hostBuilder, Action<CliOptions> options, Action<CliCenter> appender)
    /// <summary>使用 <see cref="CliHostedService"/> 服務，提供程式內命令列(<see href="https://en.wikipedia.org/wiki/Command-line_interface">Command Line Interface</see>)功能。</summary>
    /// <param name="hostBuilder">由 <see cref="Host"/> 建立出來的 <see cref="IHostBuilder"/> 執行個體。</param>
    /// <param name="options">設定 <see cref="CliOptions"/> 選項的執行函示。</param>
    /// <param name="appender">新增指令至 <see cref="CliCenter"/> 的執行函示。</param>
    /// <returns>建立 <see cref="CliHostedService"/> 服務完成後的 <see cref="IHostBuilder"/> 執行個體。</returns>
    public static IHostBuilder UseCommandLine(this IHostBuilder hostBuilder, Action<CliOptions> options, Action<CliCenter> appender)
    {
        return hostBuilder.CreateCliHostedService(options, appender);
    }
    #endregion

    #region Private Static Method : IHostBuilder CreateCliHostedService(this IHostBuilder hostBuilder, Action<CliOptions>? options = null, Action<CliCenter>? appender = null)
    /// <summary>實際建立 <see cref="CliHostedService"/> 的靜態函示。</summary>
    /// <param name="hostBuilder">由 <see cref="Host"/> 建立出來的 <see cref="IHostBuilder"/> 執行個體。</param>
    /// <param name="options">[選擇]設定 <see cref="CliOptions"/> 選項的執行函示，預設為 <see langword="null"/>。</param>
    /// <param name="appender">[選擇]新增指令至 <see cref="CliCenter"/> 的執行函示，預設為 <see langword="null"/>。</param>
    /// <returns>建立 <see cref="CliHostedService"/> 服務完成後的 <see cref="IHostBuilder"/> 執行個體。</returns>
    private static IHostBuilder CreateCliHostedService(this IHostBuilder hostBuilder, Action<CliOptions>? options = null, Action<CliCenter>? appender = null)
    {
        hostBuilder.ConfigureServices(sc =>
        {
            if (options is null)
                sc.AddSingleton<ICliHostedService, CliHostedService>(sp => new CliHostedService(opts => { }));
            else
                sc.AddSingleton<ICliHostedService, CliHostedService>(sp => new CliHostedService(options));
            sc.AddHostedService(sp =>
            {
                CliHostedService cliSvc = (CliHostedService)sp.GetRequiredService<ICliHostedService>();
                appender?.Invoke(cliSvc.Current!);
                return cliSvc;
            });
        });
        return hostBuilder;
    }
    #endregion
}
#endregion

