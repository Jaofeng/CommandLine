# 主控台指令行介面(Command Line Interface for Console Host Application) For .Net Core

## 使用方法：
```C#
var host = Host.CreateDefaultBuilder(args)
	.UseConsoleLifetime(opts => opts.SuppressStatusMessages = true)
	// 多載一：不使用 Prompt
	// .UseCommandLine()
	// 多載二：使用 Prompt, 且不需要動態綁定指令
	// .UseCommandLine(opts => opts.Prompt = $"{CurrentUser}\x1B[93m#\x1B[39m ")
	// 多載三：使用 Prompt, 且呼叫 AppendCommand 動態綁定指令
	.UseCommandLine(opts => opts.Prompt = $"{CurrentUser}\x1B[93m#\x1B[39m ", AppendCommands)
	.Build();
```
`UseCommandLine` 為 `IHostBuilder` 的擴充函示。

多載方法如下：
```C#
/// <summary>使用預設的 <see cref="CliOptions"/> 來建立 <see cref="CliHostedService"/> 服務，提供程式內命令列(<see href="https://en.wikipedia.org/wiki/Command-line_interface">Command Line Interface</see>)功能。</summary>
/// <param name="hostBuilder">由 <see cref="Host"/> 建立出來的 <see cref="IHostBuilder"/> 執行個體。</param>
/// <returns>建立 <see cref="CliHostedService"/> 服務完成後的 <see cref="IHostBuilder"/> 執行個體。</returns>
public static IHostBuilder UseCommandLine(this IHostBuilder hostBuilder);

/// <summary>使用 <see cref="CliHostedService"/> 服務，提供程式內命令列(<see href="https://en.wikipedia.org/wiki/Command-line_interface">Command Line Interface</see>)功能。</summary>
/// <param name="hostBuilder">由 <see cref="Host"/> 建立出來的 <see cref="IHostBuilder"/> 執行個體。</param>
/// <param name="options">設定 <see cref="CliOptions"/> 選項的執行函示。</param>
/// <returns>建立 <see cref="CliHostedService"/> 服務完成後的 <see cref="IHostBuilder"/> 執行個體。</returns>
public static IHostBuilder UseCommandLine(this IHostBuilder hostBuilder, Action<CliOptions> options);

/// <summary>使用 <see cref="CliHostedService"/> 服務，提供程式內命令列(<see href="https://en.wikipedia.org/wiki/Command-line_interface">Command Line Interface</see>)功能。</summary>
/// <param name="hostBuilder">由 <see cref="Host"/> 建立出來的 <see cref="IHostBuilder"/> 執行個體。</param>
/// <param name="options">設定 <see cref="CliOptions"/> 選項的執行函示。</param>
/// <param name="appender">新增指令至 <see cref="CliCenter"/> 的執行函示。</param>
/// <returns>建立 <see cref="CliHostedService"/> 服務完成後的 <see cref="IHostBuilder"/> 執行個體。</returns>
public static IHostBuilder UseCommandLine(this IHostBuilder hostBuilder, Action<CliOptions> options, Action<CliCenter> appender);
```

## 命令設定方式
使用自訂屬性 `CommandAttribute` 設定，如：
```c#
[Command("quit")]
static void CLI_Quit()
{
    _stoppingCts!.Cancel();
}
```

## `CommandAttrib` 宣告式
```C#
/// <summary>建立新的 <see cref="CommandAttribute"/> 實體類別。</summary>
/// <param name="command">CLI 指令，可使用正規表示式。</param>
/// <param name="helpText">指令說明文字。</param>
/// <param name="parent">上一層的完整指令。</param>
public CommandAttribute(string command, string helpText, string? parent = null);
```

## `CommandAttribute` 屬性
```C#
/// <summary>指令字串。</summary>
public string Command { get; private set; }
/// <summary>指令說明文字。</summary>
public string HelpText { get; private set; }
/// <summary>上層父指令。</summary>
public string? Parent { get; private set; }
/// <summary>正規表示式類型的指令說明。</summary>
public string RegularHelp { get; set; } = "";
/// <summary>是否為正規表示式類型的指令。</summary>
public bool IsRegular { get; set; } = false;
/// <summary>分類標籤。</summary>
public string? Tag { get; set; } = null;
/// <summary>是否為必要子指令。</summary>
public bool Required { get; set; } = true;
/// <summary>此指令是否為隱藏指令。</summary>
public bool Hidden { get; set; } = false;
/// <summary>子指令清單。</summary>
public CommandAttribute[] Childs { get; private set; }
/// <summary>完整指令字串。</summary>
public string FullCommand { get; private set; }
/// <summary>指令執行的方法定義。</summary>
public MethodInfo? Method { get; internal set; } = null;
```

當使用正規表示式時，需指定 `IsRegular` 屬性值為 `true`、`RegularHelp` 說明該表示式的意義。

## 函示定義
欲綁定 `CommandAttribute` 自訂屬性的函示定義格式如下：
```C#
static void CLI_Mathod1();
static void CLI_Mathod2(CliCenter cli);
static void CLI_Mathod3(CliCenter cli, params string[] args);
static void CLI_Mathod4(params string[] args);
static void CLI_Mathod5(CommandAttribute cmd, params string[] args);
static void CLI_Mathod6(CliCenter cli, CommandAttribute cmd, params string[] args);
```
以上六種方式

除了使用 `CommandAttribute` 自訂屬性的方式定義外，還可以使用 `Append` 方法新增指定。

如以下使用範例：
```C#
// File Name : Program.cs
partial class Program
{
    static CancellationTokenSource _stoppingCts;

	static void Main(string[] args)
	{
		var host = Host.CreateDefaultBuilder(args)
			.UseConsoleLifetime(opts => opts.SuppressStatusMessages = true)
			// 多載一：不使用 Prompt
			// .UseCommandLine()
			// 多載二：使用 Prompt, 且不需要動態綁定指令
			// .UseCommandLine(opts => opts.Prompt = $"{CurrentUser}\x1B[93m#\x1B[39m ")
			// 多載三：使用 Prompt, 且呼叫 AppendCommand 動態綁定指令
			.UseCommandLine(opts => opts.Prompt = $"{CurrentUser}\x1B[93m#\x1B[39m ", AppendCommands)
			.Build();
		// ......
		_stoppingCts = new CancellationTokenSource();
		var tk = host.RunAsync(_stoppingCts.Token);
		tk.Wait();
	}

    static void AppendCommands(CliCenter cli)
	{
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        MethodInfo? miHelp = typeof(Program).GetMethod("CLI_HasSubCommand", flags);

        cli.Append(new CommandAttribute("message", "訊息說明", "show"), miHelp);
        cli.Append(new CommandAttribute("message", "訊息說明", "show") { Tag = MODE_CONFIG }, miHelp);

        cli.RebindLinkRelationship();
	}
}
```


``` C#
// File Name : _CLI.cs
partial class Program
{
	#region CLI : Show Command Help - 2 Parameters: CliCenter, string[]
	[Command("show", "顯示內部資料。")]
	[Command("echo", "顯示輸入的文字。", "show")]
	static void CLI_HasSubCommand(CliCenter cli, params string[] args)
	{
		if (args[0] == "do-exec")
			args = args[1..];
		Console.WriteLine($"> 輸入 \"\x1B[96m{cli.AnalyzeToFullCommand(string.Join(" ", args))} ?\x1B[39m\" 顯示子指令清單與說明");
		Console.WriteLine();
	}
	#endregion

	#region CLI : quit - No Parameter
	[Command("quit", "關閉本程式")]
	[Command("quit", "關閉本程式", "do-exec", Tag = "config")]
	static void CLI_Quit()
	{
    	_stoppingCts!.Cancel();
	}
	#endregion

	#region CLI : exit - 1 Parameter: CliCenter
	[Command("exit", "離開設定模式", Tag = "config")]
	static void CLI_Exit(CliCenter cli)
	{
		if (string.IsNullOrEmpty(cli.UseTag)) return;
		cli.UseTag = "";
		cli.Prompt = $"{CurrentUser}\x1B[93m#\x1B[39m ";
		cli.HistoryPool = "default";
	}
	#endregion

	#region CLI : config - 1 Parameter: CliCenter
	[Command(MODE_CONFIG, "進入設定模式")]
	static void CLI_ConfigMode(CliCenter cli)
	{
		cli.UseTag = "config";
		cli.Prompt = $"{CurrentUser}(\x1B[96m{cli.UseTag}\x1B[39m)\x1B[93m#\x1B[39m ";
		Console.WriteLine();
	}
	#endregion

	#region CLI : show echo [String] - 1 Parameter: string[]
	[Command(CliCenter.STRING_REGEX, "如包含空白，請用單或雙引號。", "show echo", IsRegular = true, RegularHelp = "[String]")]
	static void CLI_Echo(params string[] args)
	{
		Console.WriteLine($"> Echo: \"\x1B[92m{args[2]}\x1B[39m\"");
		Console.WriteLine();
	}
	#endregion
}
```
