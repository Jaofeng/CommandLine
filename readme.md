# 主控台指令行介面(Command Line Interface)


[![NuGet version](https://badge.fury.io/nu/CJF.CommandLine.svg)](https://badge.fury.io/nu/CJF.CommandLine)


## 當前版本
2024-07-01 - v1.35.810
1. 新增 `CliCenter` 常數 `SBYTE_REGEX`、`BYTE_REGEX`。
2. `CommandAttribute` 新增繼承 `ICloneable` 介面，並實做其介面函示。
3. `CommandAttribute` 新增 `IsMath` 函示，用於檢查傳入的指令是否完全符合。
4. 新增 `IgnoreCase` 選項屬性，用於設定查找指令時，是否忽略大小寫。預設為 `false`，即區分大小寫。

## 引用宣告
本 `CJF.CommandLine` 部分原始碼來自 [Github](https://github.com/) [tonerdo/readline](https://github.com/tonerdo/readline/tree/master) 專案

## Github Repository
https://github.com/Jaofeng/CommandLine


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

## 設定選項類別 
```C#
/// <summary>提供 <see cref="CliHostedService"/> 的設定項目。</summary>
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
    /// <summary>開始輸入前延遲的時間，單位豪秒，預設為 1000 豪秒。</summary>
    public int Delay { get; set; } = 1000;
    /// <summary>設定或取得是否忽略大小寫。預設為 false，即大小寫視為不同。</summary>
    public bool IgnoreCase { get; set; } = false;
}
```

## 多載方法
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

## 指令設定方式
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

當 `command` 使用正規表示式時，需指定 `IsRegular` 屬性值為 `true`、`RegularHelp` 說明該表示式的意義。

內建以下正規表示式
```C#
/// <summary>一般字詞檢查式。</summary>
public const string WORD_REGEX = @"[^\d\s\.]\w+";
/// <summary>以單引號或雙引號標示的文字，或一般字詞。</summary>
public const string STRING_REGEX = @"(""[^""]*""|'[^']*'|[^\d\s\.]\w+)";
/// <summary>正整數數字檢查式。</summary>
public const string UINT_REGEX = @"\d+";
/// <summary>整數數字檢查式。</summary>
public const string INT_REGEX = @"-?\d+";
/// <summary>含小數點的數字檢查式。</summary>
public const string DECIMAL_REGEX = @"-?[0-9]+(\.[0-9]+)?";
/// <summary>UINT16 數字檢查式。</summary>
public const string UINT16_REGEX = @"(\d{1,4}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])";
/// <summary>INT16 數字檢查式。</summary>
public const string INT16_REGEX = @"(-?(\d{0,4}|[0-2]\d{4}|31\d{3}|3276[0-7])|-32768)";
/// <summary>UINT8 數字檢查式。</summary>
public const string SBYTE_REGEX = @"(-?(\d{0,2}|1[0-1]\d|12[0-7])|-128)";
/// <summary>BYTE 數字檢查式。</summary>
public const string BYTE_REGEX = @"(\d{1,2}|1\d{2}|2[0-4]\d|25[0-5])";
/// <summary>16 進位字串檢查式。</summary>
public const string HEX_REGEX = @"[0-9a-fA-F]+";
/// <summary>1 位元組的 16 進位字串檢查式。</summary>
public const string HEX1BYTE_REGEX = @"[0-9a-fA-F]{2}";
/// <summary>2 位元組的 16 進位字串檢查式。</summary>
public const string HEX2BYTE_REGEX = @"[0-9a-fA-F]{4}";
/// <summary>IP 位址檢查式。</summary>
public const string IP_REGEX = @"((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.|\s?|$)){4}";
/// <summary>通訊埠號檢查式。</summary>
public const string PORT_REGEX = UINT16_REGEX;
/// <summary>電話檢查式。</summary>
public const string PHONE_REGEX = @"(09\d{2}-*\d{3}-*\d{3}|\(*0\d\)*-*\d{3,4}-*\d{4})";
/// <summary>信箱檢查式。</summary>
public const string EMAIL_REGEX = @"\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z]+";
/// <summary>密碼驗證規則。</summary>
/// <remarks>
/// <para>1. 至少一個英文字母 (?=.*?[A-Za-z])。</para>
/// <para>2. 至少一個數字(?=.*?[0-9])。</para>
/// <para>3. 長度至少為 8 個字元.{8,}。</para>
/// </remarks>
public const string PWD_REGEX = @"(?=.*?[A-Za-z])(?=.*?[0-9]).{8,}";
/// <summary>密碼驗證規則。</summary>
/// <remarks>
/// <para>1. 至少一個大寫英文字母 (?=.*?[A-Z])。</para>
/// <para>2. 至少一個小寫的英文字母 (?=.*?[a-z])。</para>
/// <para>3. 至少一個數字 (?=.*?[0-9])。</para>
/// <para>4. 至少一個特殊字元 (?=.*?[#?!@$%^&amp;*-])。</para>
/// <para>5. 長度至少為 8 個字元 .{8,}。</para>
/// </remarks>
public const string PWD_REGEX_COMPLEX = @"(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}";
```

## 函示定義
綁定 `CommandAttribute` 自訂屬性的函示需定義為 static 類型，支援的格式如下：
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

## 使用範例
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
為加快指令尋找、比對的正確性，在使用 `Append` 新增指令後，請使用 `RebindLinkRelationship` 方法重新建立指令的連結關係。

``` C#
// File Name : _CLI.cs
partial class Program
{
	#region CLI : Show Command Help - 2 Parameters: CliCenter, string[]
	// 當輸入不完整的指令時，顯示的內容
	// 輸入 "show" 時，顯示的內容為 "> 輸入 "show ?" 顯示子指令清單與說明"
	// 輸入 "show echo" 時，顯示的內容為 "> 輸入 "show echo ?" 顯示子指令清單與說明"
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
	// 輸入 show echo "Test String" 時，會顯示 "> Echo: "Test String""
	[Command(CliCenter.STRING_REGEX, "如包含空白，請用單或雙引號。", "show echo", IsRegular = true, RegularHelp = "[String]")]
	static void CLI_Echo(params string[] args)
	{
		Console.WriteLine($"> Echo: \"\x1B[92m{args[2]}\x1B[39m\"");
		Console.WriteLine();
	}
	#endregion
}
```

## 範例結果
![範例結果](https://i.imgur.com/AbY81PB.png)

---
## 歷史版本紀錄
2024-06-28 - v1.34.795
1. 修正無法過濾標籤(UseTag)的問題。
2. 修正特定指令重複定義時會發生錯誤的問題。

2023-07-16 - v1.33.762
1. 修正訊息輸出時，按上下鍵時會產生錯誤而中斷執行的問題。

2023-07-10 - v1.33.752
1. 新增 `Delay` 選項屬性，可指定開始輸入前暫停的時間，預設 `1` 秒。
2. 新增 `Pause` 屬性，設定為 `true` 時，將無法輸入，直到變更為 `false` 。

2023-07-06 - v1.33.740
1. 優化內建的正規表示式規則
2. 優化指令層級並加強指令彈性
3. 增加 `DebugMode` 顯示訊息，並於程式執行後，以樹狀結構顯示指令與其對應之函示名

2023-06-27 - v1.32.725
1. 新增除錯模式，於 `CliOptions` 中設定；設定後，會在指令執行前顯示完整指令以及執行的函示名稱。
2. 檢驗時，遇到兩個以上模糊曖昧的規則時，預設會執行第一個非正規表示式的規則，但最好避免此種狀況。

2023-06-05 - v1.31.710
1. 新增密碼字元設定選項

2023-05-26 - v1.30.702.200526
1. 首次發布
