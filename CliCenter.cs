using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace CJF.CommandLine;

#region Public Sealed Class : CliCenter
/// <summary>CLI 指定控制中心的集合類別。</summary>
[UnsupportedOSPlatform("browser")]
public sealed class CliCenter
{
    #region Public Consts
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
    /// <summary>ANSI Key codes</summary>
    public const string ANSI_PATTERN = "\x1B\\[(\\d+)*;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?([ABCDEFGHJKSTfmsu])";
    /// <summary>預設的歷史指令清單分類名稱。</summary>
    public const string DEFAULT_POOL = "default";
    #endregion

    #region Public Events
    /// <summary>自 <see cref="Reader.Read"/> 接收到指令時所產生的事件回呼。</summary>
    public event CommandEnterHandler? CommandEntered;
    /// <summary>執行對應指令前所產生的事件回呼。</summary>
    public event ExecuteCommandHandler? BeforeMethodExecute;
    /// <summary>執行對應指令後所產生的事件回呼。</summary>
    public event ExecuteCommandHandler? AfterMethodExecute;
    #endregion

    #region Public Properties
    /// <summary>傳回列管的指令數量。</summary>
    public int Count => _Commands.Count;    // _items.Count;
    /// <summary>傳回列管的指令清單。</summary>
    public IEnumerable<CommandAttribute> Commands
    {
        get
        {
            //CommandAttribute[] cas = _items.Keys.ToArray();
            CommandAttribute[] cas = _Commands.ToArray();
            Array.Sort(cas, (a, b) =>
            {
                if (a.Tag is null && b.Tag is not null)
                    return -1;
                else if (a.Tag is not null && b.Tag is null)
                    return 1;
                else
                {
                    if (a.Tag != b.Tag)
                        return a.Tag!.CompareTo(b.Tag);
                    else
                        return a.FullCommand.CompareTo(b.FullCommand);
                }

            });
            foreach (CommandAttribute ca in cas)
                yield return ca;
        }
    }
    /// <summary>設定或取得 CLI 的指令提示字串。</summary>
    public string Prompt { get; set; } = string.Empty;
    /// <summary>CLI 的指令提示字串的顯示顏色。</summary>
    public ConsoleColor PromptColor { get; set; } = Console.ForegroundColor;
    /// <summary>設定或取得密碼輸入時的顯示字元。</summary>
    public static char? PasswordChar { get; set; }
    /// <summary>設定或取得是否啟用除錯模式。</summary>
    public bool DebugMode { get; set; } = false;
    /// <summary>設定或取得目前使用的分類標籤字串。</summary>
    public string UseTag { get; set; } = string.Empty;
    /// <summary>設定或取得歷史指令清單的分類名稱。</summary>
#pragma warning disable CA1822 // Mark members as static
    public string HistoryPool
    {
        get => Reader.PoolName;
        set => Reader.SetPool(value);
    }
#pragma warning restore CA1822 // Mark members as static
    /// <summary>取得目前當下正在執行的指令。</summary>
    public CommandAttribute? ExecutingCommand { get; private set; }
    #endregion

    #region Private Variables
    private static readonly Regex QuotesRegex = new("(\"[^\"]*\"|'[^']*')");
    private static readonly Regex AnsiRegex = new(ANSI_PATTERN);
    //private readonly Dictionary<CommandAttribute, MethodInfo> _items;
    private readonly List<CommandAttribute> _Commands;
    private Func<string, int, string[]?>? _CommandSuggestions;
    private Task? _executeTask;
    private CancellationTokenSource? _cancellationTokenSource;
    #endregion


    #region Public Construct Method : CliCenter()
    /// <summary>建立新的 <see cref="CliCenter"/> 執行個體。</summary>
    public CliCenter()
    {
        _Commands = new List<CommandAttribute>();
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type t in a.GetTypes().Where(_t => _t.GetRuntimeMethods().Any(_m => _m.GetCustomAttributes<CommandAttribute>().Any())))
                Join(t, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        RebindLinkRelationship();
        HistoryPool = DEFAULT_POOL;
        KeyHandler.PrintCommandsHandler = PrintCommands;
        SetCommandSuggestions(null);
    }
    #endregion

    #region Public Construct Method : CliCenter(string prompt, ConsoleColor? promptColor = null, string historyPool = DEFAULT_POOL, char? pwdChar = null, bool debug = false)
    /// <summary>建立新的 <see cref="CliCenter"/> 執行個體。</summary>
    /// <param name="prompt">指令提示字串。</param>
    /// <param name="promptColor">指令提示字串顏色。</param>
    /// <param name="historyPool">歷史指令清單的分類字串，預設值為 <see cref="DEFAULT_POOL"/>。</param>
    /// <param name="pwdChar">密碼顯示字元。</param>
    /// <param name="debug">是否啟用除錯模式。</param>
    public CliCenter(string prompt, ConsoleColor? promptColor = null, string historyPool = DEFAULT_POOL, char? pwdChar = null, bool debug = false) : this()
    {
        Prompt = prompt;
        HistoryPool = historyPool;
        PromptColor = promptColor ?? Console.ForegroundColor;
        PasswordChar = Reader.PasswordChar = pwdChar;
        DebugMode = debug;
    }
    #endregion

    #region Internal Construct Method : CliCenter(CliOptions opts)
    /// <summary>建立新的 <see cref="CliCenter"/> 執行個體，本建立式僅供 <see cref="CliHostedService"/> 使用。</summary>
    /// <param name="opts">供 <see cref="CliHostedService"/> 傳遞用的設定類別。</param>
    internal CliCenter(CliOptions opts) : this(opts.Prompt, opts.PromptColor, opts.HistoryPool, opts.PasswordChar, opts.DebugMode) { }
    #endregion


    #region Public Static Method : string[] SplitCommand(string text)
    /// <summary>從字串中切割出各個指令。</summary>
    /// <param name="text">欲切割的原始字串。</param>
    /// <param name="removeQuotes">傳回指令陣列時，是否移除內含的單引號(')和雙引號(")。</param>
    /// <returns>切割完畢的指令。</returns>
    public static string[] SplitCommand(string text, bool removeQuotes = false)
    {
        var reg = QuotesRegex;
        string[] arr;
        if (reg.IsMatch(text))
        {
            var res = new List<string>();
            string tmp = text;
            Match _m;
            while ((_m = reg.Match(tmp)) is not null && _m.Success)
            {
                arr = tmp[.._m.Index].Split(' ');
                if (res.Count == 0)
                    res.AddRange(arr);
                else
                {
                    if (string.IsNullOrWhiteSpace(arr[0]))
                        res[^1] += arr[0];
                    else
                        res.Add(arr[0]);
                    res.AddRange(arr);
                    res.RemoveAt(res.Count - arr.Length);
                }
                if (_m.Index != 0 && tmp[_m.Index - 1] == ' ')
                    res[^1] += removeQuotes ? _m.Value.Trim("'\"".ToCharArray()) : _m.Value;
                else
                    res.Add(_m.Value);
                tmp = tmp[(_m.Index + _m.Length)..].TrimStart();
            }
            if (!string.IsNullOrWhiteSpace(tmp))
                res.AddRange(tmp.Split(' '));
            return res.ToArray();
        }
        else
        {
            return text.Split(' ').Where(_t => !string.IsNullOrWhiteSpace(_t)).ToArray();
        }
    }
    #endregion


    #region Public Method : void Start()
    /// <summary>以同步方式開始執行 <see cref="CliCenter"/> 執行個體。</summary>
    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        StartAsync(_cancellationTokenSource.Token).Wait();
    }
    #endregion

    #region Public Method : Task StartAsync(CancellationToken cancellationToken)
    /// <summary>以非同步方式開始執行 <see cref="CliCenter"/> 執行個體。</summary>
    /// <param name="cancellationToken">自外部傳入中斷執行的通知。</param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executeTask = WorkerProcess(cancellationToken);
        if (_executeTask.IsCompleted)
            return _executeTask;
        return Task.CompletedTask;
    }
    #endregion

    #region Public Method : void Stop()
    /// <summary>停止執行 <see cref="CliCenter"/> 執行個體。</summary>
    public void Stop()
    {
        if (_cancellationTokenSource is null) return;
        _cancellationTokenSource!.Cancel();
        StopAsync(_cancellationTokenSource!.Token).Wait();
    }
    #endregion

    #region Public Method : async Task StopAsync(CancellationToken cancellationToken)
    /// <summary>以非同步方式停止 <see cref="CliCenter"/> 執行個體。</summary>
    /// <param name="cancellationToken">自外部傳入中斷執行的通知。</param>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executeTask == null) return;
        await Task.WhenAny(_executeTask, Task.Delay(1000, cancellationToken)).ConfigureAwait(false);
    }
    #endregion

    #region Public Method : void Append(CommandAttribute cmd, MethodInfo method)
    /// <summary>新增指令與其定義的函示。</summary>
    /// <param name="cmd">指令類別。</param>
    /// <param name="method">指令綁定的函示。</param>
    /// <exception cref="CommandRegularHelpEmptyException">正規表示式的指令說明文字未定義。</exception>
    /// <exception cref="CommandDuplicateException">指令重複。</exception>
    public void Append(CommandAttribute cmd, MethodInfo method)
    {
        if (_Commands.Find(_ca => _ca.Match(cmd)) is null)
        {
            if (cmd.IsRegular && string.IsNullOrEmpty(cmd.RegularHelp))
            {
                if (method.DeclaringType is not null)
                    throw new CommandRegularHelpEmptyException($"\"{cmd.Command}\" in methodInfo \"{method.Name}\" from \"{method.DeclaringType.FullName}\" using regular expressions, but the \"{nameof(cmd.RegularHelp)}\" property is empty!");
                else
                    throw new CommandRegularHelpEmptyException($"\"{cmd.Command}\" in methodInfo \"{method.Name}\" using regular expressions, but the \"{nameof(cmd.RegularHelp)}\" property is empty!");
            }
            else
            {
                cmd.Method = method;
                _Commands.Add(cmd);
            }
        }
        else
        {
            if (method.DeclaringType is not null)
                throw new CommandDuplicateException($"\"{cmd.Command}\" is duplicate in methodInfo \"{method.Name}\" from \"{method.DeclaringType.FullName}\"");
            else
                throw new CommandDuplicateException($"\"{cmd.Command}\" is duplicate in methodInfo \"{method.Name}\"");
        }
    }
    #endregion

    #region Public Method : void RebindLinkRelationship()
    /// <summary>重新綁定連結所有指令(<see cref="CommandAttribute"/>)的關係。</summary>
    public void RebindLinkRelationship()
    {

        foreach (CommandAttribute ca in _Commands.Where(_ca => string.IsNullOrEmpty(_ca.Parent)))
            SetChilds(ca);
        if (DebugMode)
            PrintCommandTree();
    }
    #endregion

    #region Public Method : bool RemoveCommand(string command)
    /// <summary>移除指令。</summary>
    /// <param name="command">CLI 指令，可使用縮寫指令。</param>
    /// <returns>如成功刪除指令，將回傳 <see langword="true"/>, 否則為 <see langword="false"/>。</returns>
    public bool RemoveCommand(string command)
    {
        if (GetCommand(command) is not CommandAttribute cmd)
            return false;
        else
            return _Commands.Remove(cmd);
    }
    #endregion

    #region Public Method : void Clear()
    /// <summary>清除所有指令。</summary>
    public void Clear() => _Commands.Clear();
    #endregion

    #region Public Method : bool TryGetMethod(string command, out MethodInfo? method)
    /// <summary>以傳入的指令字串嘗試取得其綁定的函示。</summary>
    /// <param name="command">CLI 指令，可使用縮寫指令。</param>
    /// <param name="method">傳回綁定的函示。</param>
    /// <returns>如找到綁定的函示，將回傳 <see langword="true"/>, 否則為 <see langword="false"/>。</returns>
    public bool TryGetMethod(string command, out MethodInfo? method)
    {
        if (TryGetCommand(command, out CommandAttribute? cca) && cca is not null)
        {
            method = cca.Method;
            return method is not null;
        }
        else
        {
            method = null;
            return false;
        }
    }
    #endregion

    #region Public Method : bool TryGetCommand(string command, out CommandAttribute? ca)
    /// <summary>嘗試取得指令屬性資料。</summary>
    /// <param name="command">CLI 指令，可使用縮寫指令。</param>
    /// <param name="cmd">[傳回]取得的指令屬性資料。</param>
    /// <returns>如正確取得指令屬性資料，將回傳 <see langword="true"/>，否則為 <see langword="false"/>。</returns>
    public bool TryGetCommand(string command, out CommandAttribute? cmd)
    {
        cmd = GetCommand(command);
        return cmd is not null;
    }
    #endregion

    #region Public Method : IEnumerable<string> GetCommandNames()
    /// <summary>取得所有列管的指令名稱。</summary>
    /// <returns>所有列管的指令名稱清單。</returns>
    public IEnumerable<string> GetCommandNames()
    {
        foreach (CommandAttribute ca in _Commands)
            yield return ca.FullCommand;

    }
    #endregion

    #region Public Method : string? AnalyzeToFullCommand(string command)
    /// <summary>分析輸入的字串，可傳入縮寫指令，會傳回完整指令字串。</summary>
    /// <param name="command">CLI 指令，可使用縮寫指令。</param>
    /// <returns>完整指令字串。</returns>
    public string? AnalyzeToFullCommand(string command)
    {
        string res = "";
        CommandAttribute? cmd = null;
        foreach (string s in SplitCommand(command))
        {
            if (string.IsNullOrWhiteSpace(s)) continue;
            if (string.IsNullOrEmpty(res))
            {
                var _cas = _Commands.Where(_ca => string.IsNullOrEmpty(_ca.Parent) && _ca.Command.StartsWith(s));
                if (!_cas.Any()) return null;
                cmd = _cas.First();
                res = cmd.Command;
            }
            else
            {
                #region Predicate Method : bool _FindCommandAttrinute(CommandAttribute ca)
                bool _FindCommandAttrinute(CommandAttribute ca)
                {
                    if (string.IsNullOrEmpty(ca.Parent)) return false;
                    return ca.IsRegular && Regex.IsMatch(s, $"^{ca.Command}$") && Regex.IsMatch(res, $"^{ca.Parent}$") ||
                        !ca.IsRegular && ca.Command.StartsWith(s) && ca.Parent.Equals(res);
                }
                #endregion

                if (cmd!.Childs.FirstOrDefault(_FindCommandAttrinute) is CommandAttribute _ca)
                {
                    if (_ca.IsRegular)
                        res += " " + s;
                    else
                        res += " " + _ca.Command;
                    cmd = _ca;
                }
                else
                    break;

            }
        }
        return res;

    }
    #endregion

    #region Public Method : CommandAttribute? GetParentCommand(CommandAttribute command)
    /// <summary>取得上層父指令。</summary>
    /// <param name="command">欲取得父指令的指令類別。</param>
    /// <returns>取得的上層父指令，如無父指令則回傳 null。</returns>
    public CommandAttribute? GetParentCommand(CommandAttribute command)
    {
        if (string.IsNullOrEmpty(command.Parent)) return null;
        //return _items.Keys.FirstOrDefault(_ca => _ca.FullCommand == command.Parent);
        return _Commands.FirstOrDefault(_ca => _ca.FullCommand == command.Parent);
    }
    #endregion

    #region Public Method : string[]? GetSuggestions(string command, int index)
    /// <summary>取得指令建議的函示定義。</summary>
    /// <param name="command">CLI 指令，可使用縮寫指令。</param>
    /// <param name="index">指令欄位索引值。</param>
    /// <returns>建議的指令清單。</returns>
    public string[]? GetSuggestions(string command, int index)
    {
        if (GetCommands(command, true) is IEnumerable<CommandAttribute> cmd)
            return cmd.Select(_ca => _ca.Command).ToArray();
        else
            return null;
    }
    #endregion

    #region Public Method : void ShowHelp(string command, string subCmd = "")
    /// <summary>顯示指令說明。</summary>
    /// <param name="command">CLI 指令，可使用縮寫指令。</param>
    /// <param name="subCmd">子指令。</param>
    public void ShowHelp(string command, string subCmd = "")
    {
        if (!TryGetCommand(command, out CommandAttribute? cca) || cca is null)
        {
            if (GetCommands(command).Any())
                Console.WriteLine($"% Ambiguous command: \"{command} {(string.IsNullOrEmpty(subCmd) ? "" : subCmd.TrimEnd('?'))}\"");
            else
                Console.WriteLine($"% Unknow command: \"{command} {(string.IsNullOrEmpty(subCmd) ? "" : subCmd.TrimEnd('?'))}\"");
            Console.WriteLine("");
            return;
        }

        IEnumerable<CommandAttribute> _cas;
        if (string.IsNullOrEmpty(command))
        {
            if (string.IsNullOrEmpty(UseTag))
                _cas = _Commands.Where(_ca => string.IsNullOrEmpty(_ca.Parent) && string.IsNullOrEmpty(_ca.Tag));
            else
                _cas = _Commands.Where(_ca => string.IsNullOrEmpty(_ca.Parent) && (string.IsNullOrEmpty(_ca.Tag) || _ca.Tag == UseTag));
        }
        else if (string.IsNullOrEmpty(subCmd))
            _cas = cca.Childs;
        else
            _cas = GetChildCommands(cca, subCmd);
        int _m = 15;
        if (_cas is not null && _cas.FirstOrDefault() is not null)
        {
            _cas = _cas.Where(_ca => !_ca.Hidden).OrderBy(_ca => _ca.IsRegular ? _ca.RegularHelp : _ca.Command);
            _m = _cas.Max(_ca => _ca.IsRegular ? _ca.RegularHelp.Length : _ca.Command.Length);
            foreach (CommandAttribute ca in _cas)
            {
                if (ca.IsRegular)
                    Console.WriteLine($"  {ca.RegularHelp.PadRight(_m + 5)}{ca.HelpText}");
                else
                    Console.WriteLine($"  {ca.Command.PadRight(_m + 5)}{ca.HelpText}");
            }
        }
        if (!string.IsNullOrEmpty(command) && !cca.Childs.Any(_c => _c.Required))
            Console.WriteLine($"  {"<cr>".PadRight(_m + 5)}<cr>");
        Console.WriteLine();
    }
    #endregion

    #region Public Method : void SetCommandSuggestions(Func<string, int, string[]?>? func)
    /// <summary>設定指令建議函示。</summary>
    /// <param name="func">指令建議含式委派的繫結。</param>
    public void SetCommandSuggestions(Func<string, int, string[]?>? func)
    {
        if (func is null)
            _CommandSuggestions = GetSuggestions;
        else
            _CommandSuggestions = func;
        Reader.AutoCompletionHandler = _CommandSuggestions;
    }
    #endregion

    #region Public Method : string ReadLine(string prompt)
    /// <summary>讀取一行文字。</summary>
    /// <param name="prompt">提示文字。</param>
    /// <returns>使用者輸入的文字字串。</returns>
    public string ReadLine(string prompt)
    {
        var res = Reader.Read(prompt, PromptColor);
        Reader.RemoveLastHistory();
        return res;
    }
    #endregion

    #region Public Method : string ReadPassword(string prompt)
    /// <summary>讀取密碼。</summary>
    /// <param name="prompt">提示文字。</param>
    /// <returns>使用者輸入的密碼。
    public string ReadPassword(string prompt) => Reader.ReadPassword(prompt, PasswordChar);
    #endregion


    #region Internal Method : Task WorkerProcess(CancellationToken cancellationToken)
    /// <summary>指令等待背景執行緒。</summary>
    internal Task WorkerProcess(CancellationToken cancellationToken)
    {
        string? _full;
        ConsoleColor _OrigColor = Console.ForegroundColor;
        string cmd = Reader.Read(Prompt, PromptColor).Trim();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (string.IsNullOrWhiteSpace(cmd))
            {
                cmd = Reader.Read(Prompt, PromptColor).Trim();
                continue;
            }
            OnCommandEntered(cmd);
            _full = AnalyzeToFullCommand(cmd);
            if (DebugMode)
                Console.WriteLine($"\x1B[90m[DEBUG]\x1B[39m Full Command: \x1B[92m{_full}\x1B[39m");
            if (_full is null)
            {
                if (cmd.EndsWith('?'))
                    HandleQuestionMark(cmd);
                else
                {
                    Console.Write("".PadLeft(AnsiRegex.Replace(Prompt, "").Length));
                    Console.WriteLine("^");
                    Console.WriteLine("% Invalid input detected at '^' marker.");
                }
            }
            else if (SplitCommand(cmd).Length != SplitCommand(_full).Length)
            {
                if (cmd.EndsWith('?'))
                    HandleQuestionMark(cmd);
                else
                {
                    Console.Write("".PadLeft(AnsiRegex.Replace(Prompt, "").Length));
                    Console.Write("".PadLeft(string.Join(" ", SplitCommand(cmd), 0, SplitCommand(_full).Length).Length + 1));
                    Console.WriteLine("^");
                    Console.WriteLine("% Invalid input detected at '^' marker.");
                }
            }
            else
            {
                IEnumerable<CommandAttribute> cas = GetCommands(cmd);
                if (cas.Any())
                {
                    CommandAttribute fca = cas.First();
                    if (cmd.EndsWith('?'))
                        HandleQuestionMark(cmd);
                    else if (cas.Count() == 1)
                        InvokeCommandMethod(_full, fca);
                    else
                    {
                        if (DebugMode)
                        {
                            Console.WriteLine($"\x1B[90m[DEBUG]\x1B[39m Ambiguous command: \"\x1B[96m{cmd}\x1B[39m\"");
                            foreach (CommandAttribute _ca in cas)
                                Console.WriteLine($"\x1B[90m[DEBUG]\x1B[39m   \x1B[96m{_ca.FullCommand}\x1B[39m  >>  \x1B[93m{_ca.Method!.Name}\x1B[39m");
                        }
                        if (cas.FirstOrDefault(_ca => !_ca.IsRegular) is CommandAttribute ca)
                            InvokeCommandMethod(_full, ca);
                        else
                            Console.WriteLine($"% Ambiguous command: \"\x1B[96m{cmd}\x1B[39m\"");
                    }
                }
                else
                {
                    Console.Write("".PadLeft(AnsiRegex.Replace(Prompt, "").Length));
                    Console.WriteLine("^");
                    Console.WriteLine("% Invalid input detected at '^' marker.");
                }
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                if (cmd.EndsWith('?'))
                    cmd = Reader.Read(Prompt, PromptColor, cmd.Remove(cmd.Length - 1)).Trim();
                else
                    cmd = Reader.Read(Prompt, PromptColor).Trim();
            }
        }
        Console.ForegroundColor = _OrigColor;
        return Task.CompletedTask;
    }
    #endregion


    #region Private Method : bool InvokeCommandMethod(string fullCommand, CommandAttribute cmdAttr)
    private bool InvokeCommandMethod(string fullCommand, CommandAttribute cmdAttr)
    {
        if (cmdAttr.Method is null) return false;
        if (OnBeforeMethodExecute(cmdAttr, cmdAttr.Method))
        {
            object[] args;
            var cmds = SplitCommand(fullCommand, true);
            var _params = cmdAttr.Method.GetParameters();
            if (_params.Length == 3 && _params[0].ParameterType == typeof(CliCenter) && _params[1].ParameterType == typeof(CommandAttribute) && _params[2].ParameterType == typeof(string[]))
                args = new object[] { this, cmdAttr, cmds };
            else if (_params.Length == 2 && _params[0].ParameterType == typeof(CliCenter) && _params[1].ParameterType == typeof(string[]))
                args = new object[] { this, cmds };
            else if (_params.Length == 2 && _params[0].ParameterType == typeof(CommandAttribute) && _params[1].ParameterType == typeof(string[]))
                args = new object[] { cmdAttr, cmds };
            else if (_params.Length == 1 && _params[0].ParameterType == typeof(CliCenter))
                args = new object[] { this };
            else if (_params.Length == 1 && _params[0].ParameterType == typeof(CommandAttribute))
                args = new object[] { cmdAttr };
            else if (_params.Length == 1 && _params[0].ParameterType == typeof(string[]))
                args = new object[] { cmds };
            else
                args = Array.Empty<object>();
            try
            {
                if (DebugMode)
                    Console.WriteLine($"\x1B[90m[DEBUG]\x1B[39m Executing \x1B[96m{fullCommand}\x1B[39m >> \x1B[93m{cmdAttr.Method.Name}\x1B[39m");
                if (cmdAttr.Method.IsStatic)
                    cmdAttr.Method.Invoke(null, args);
                else
                {
                    var instance = Activator.CreateInstance(cmdAttr.Method.DeclaringType!);
                    cmdAttr.Method.Invoke(instance, args);
                }
            }
            catch (TargetException)
            {
                Console.WriteLine($"\x1B[91m[!]\x1B[39m 函示 \x1B[32m{cmdAttr.Method.Name}\x1B[39m 宣告錯誤!");
                Console.WriteLine(cmdAttr.Method);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\x1B[91m[!]\x1B[39m 函示 \x1B[32m{cmdAttr.Method.Name}\x1B[39m 執行錯誤!");
                Console.WriteLine(ex);
                return false;
            }
            if (!OnAfterMethodExecute(cmdAttr, cmdAttr.Method)) return false;
            return true;
        }
        else
            return false;
    }
    #endregion

    #region Private Method : void HandleQuestionMark(string text)
    /// <summary>處理問號符號(?)。</summary>
    /// <param name="text">當前輸入的完整指令字串。</param>
    private void HandleQuestionMark(string text)
    {
        // Show Help or commands
        Reader.RemoveLastHistory();
        int _idx = text.LastIndexOf(' ');
        if (text.Length == 1)
            ShowHelp("");
        else if (_idx == -1 || !string.IsNullOrEmpty(text[(_idx + 1)..].TrimEnd('?')))
        {
            // First Command, exp: sh?
            string[]? _cs = null;
            if (_CommandSuggestions is not null)
            {
                //_cs = _CommandSuggestions.Invoke(text.Substring(0, text.Length - 1), 0);
                _cs = _CommandSuggestions.Invoke(text[..^1], 0);
            }
            if (_cs is null || _cs.Length == 0)
                Console.WriteLine("%  Unrecognized command");
            else
                PrintCommands(_cs);
        }
        else
        {
            // Another command, exp: sh ?
            ShowHelp(text[.._idx], text[(_idx + 1)..].TrimEnd('?'));
        }
    }
    #endregion

    #region Private Method : void Join(Type fromClass, BindingFlags binding)
    /// <summary>連結特定類別的與其類型的指令函示。</summary>
    /// <param name="fromClass">欲綁定的特定類別類型。</param>
    /// <param name="binding">該特定類別函示的繫結旗標。</param>
    private void Join(Type fromClass, BindingFlags binding)
    {
        foreach (MethodInfo mi in fromClass.GetMethods(binding).Where(_m => _m.IsStatic && _m.GetCustomAttributes<CommandAttribute>().Any()))
        {
            foreach (CommandAttribute cmd in mi.GetCustomAttributes<CommandAttribute>())
            {
                if (!cmd.IsValid())
                    throw new CommandArgumentException($"Command: \"{cmd.Command}\" 定義錯誤!");
                Append(cmd, mi);
            }
        }
    }
    #endregion

    #region Private Method : void SetChilds(CommandAttribute cmd)
    /// <summary>將指令綁定成子指令。</summary>
    /// <param name="cmd">欲綁定的指令。</param>
    private void SetChilds(CommandAttribute cmd)
    {
        cmd.ClearChilds();
        cmd.Level = (string.IsNullOrEmpty(cmd.Parent) ? 0 : SplitCommand(cmd.Parent).Length) + 1;
        string _pc = string.IsNullOrEmpty(cmd.Parent) ? cmd.Command : $"{cmd.Parent} {cmd.Command}";
        foreach (CommandAttribute ca in _Commands.Where(_ca => !string.IsNullOrEmpty(_ca.Parent) && (Regex.IsMatch(_pc, $"^{_ca.Parent}$") || _pc == _ca.Parent) && _ca.Tag == cmd.Tag))
        {
            if (!cmd.Childs.Contains(ca))
                cmd.AddChild(ca);
            SetChilds(ca);
        }
    }
    #endregion

    #region Private Method : CommandAttribute? GetCommand(string command)
    /// <summary>以指令字串取得指令類別。</summary>
    /// <param name="command">CLI 指令字串。</param>
    /// <returns>找到的指令類別，否則為 null。</returns>
    private CommandAttribute? GetCommand(string command)
    {
        CommandAttribute? cmd = null;
        var lca = new List<CommandAttribute>();
        if (!command.Contains(' '))
        {
            CommandAttribute? ca;
            if (string.IsNullOrEmpty(UseTag))
                ca = _Commands.FirstOrDefault(_ca => string.IsNullOrEmpty(_ca.Parent) && _ca.Command.StartsWith(command) && string.IsNullOrEmpty(_ca.Tag));
            else
                ca = _Commands.FirstOrDefault(_ca => string.IsNullOrEmpty(_ca.Parent) && _ca.Command.StartsWith(command) && (string.IsNullOrEmpty(_ca.Tag) || _ca.Tag == UseTag));
            if (ca is not null) cmd = ca;
        }
        else
        {
            string[] _cs = SplitCommand(command);
            if (!TryGetCommand(_cs[0], out CommandAttribute? _fca) || _fca is null) return null;
            int idx = 1;
            CommandAttribute pca = _fca;
            CommandAttribute[] cas;
            while (idx < _cs.Length)
            {
                cas = GetChildCommands(pca, _cs[idx]).ToArray();
                if (cas.Length != 1)
                {
                    CommandAttribute[] _cas = cas.Where(_ca => !_ca.IsRegular && _ca.Command.StartsWith(_cs[idx])).ToArray();
                    if (_cas.Length == 1)
                    {
                        pca = _cas[0];
                        idx++;
                        continue;
                    }
                    else if (_cas.Length == 0)
                    {
                        _cas = cas.Where(_ca => _ca.IsRegular && Regex.IsMatch(_cs[idx], $"^{_ca.Command}$")).ToArray();
                        if (_cas.Length == 1)
                        {
                            pca = _cas[0];
                            idx++;
                            continue;
                        }
                        else
                            break;
                    }
                    else
                        break;
                }
                pca = cas[0];
                idx++;
            }
            cmd = pca;
        }
        return cmd;
    }
    #endregion

    #region Private Method : IEnumerable<CommandAttribute> GetCommands(string command, bool ignoreRegular = false)
    /// <summary>找尋相似的指定清單。</summary>
    /// <param name="command">指令前置字串。</param>
    /// <param name="ignoreRegular">是否忽略正規表示式類型的指令類別。</param>
    /// <returns>找到的指令清單。</returns>
    private IEnumerable<CommandAttribute> GetCommands(string command, bool ignoreRegular = false)
    {
        var lca = new List<CommandAttribute>();
        if (!command.Contains(' '))
        {
            if (string.IsNullOrEmpty(UseTag))
                lca.AddRange(_Commands.Where(_ca => string.IsNullOrEmpty(_ca.Parent) && _ca.Command.StartsWith(command) && string.IsNullOrEmpty(_ca.Tag)));
            else
                lca.AddRange(_Commands.Where(_ca => string.IsNullOrEmpty(_ca.Parent) && _ca.Command.StartsWith(command) && (string.IsNullOrEmpty(_ca.Tag) || _ca.Tag == UseTag)));
        }
        else
        {
            string[] _cs = SplitCommand(command);
            if (!TryGetCommand(_cs[0], out CommandAttribute? _fca) || _fca is null) return Enumerable.Empty<CommandAttribute>();
            int idx = 1;
            CommandAttribute pca = _fca;
            while (idx < _cs.Length)
            {
                if (!__Append(pca, out CommandAttribute[] _cas))
                {
                    lca.AddRange(_cas);
                    break;
                }
                idx++;
                foreach (CommandAttribute ca in _cas)
                {
                    if (!__Append(ca, out CommandAttribute[] __cas))
                        lca.AddRange(__cas);
                }
                if (idx == _cs.Length - 1)
                    break;
                else if (_cas.Length == 1)
                    pca = _cas[0];
                else
                    break;
            }
            bool __Append(CommandAttribute ca, out CommandAttribute[] cas)
            {
                if (idx == _cs.Length - 1 && ignoreRegular)
                    cas = GetChildCommands(ca, _cs[idx]).Where(_ca => !_ca.IsRegular).ToArray();
                else
                    cas = GetChildCommands(ca, _cs[idx]).ToArray();
                return idx != _cs.Length - 1;
            }
        }
        return lca;
    }
    #endregion


    #region Private Static Method : IEnumerable<CommandAttribute> GetChildCommands(CommandAttribute parent, string word)
    /// <summary>取得以特定的指令前置字串，找尋符合的子指令。</summary>
    /// <param name="parent">上層父指令。</param>
    /// <param name="word">子指令的前置字串。</param>
    /// <returns>符合的子指令清單。</returns>
    private static IEnumerable<CommandAttribute> GetChildCommands(CommandAttribute parent, string word)
    {
        return parent.Childs.Where(_ca => (!_ca.IsRegular && _ca.Command.StartsWith(word)) || (_ca.IsRegular && Regex.IsMatch(word, $"^{_ca.Command}$")));
    }
    #endregion


    #region Private Method : void OnCommandEntered(string command)
    /// <summary>於介面上輸入指令並按下 Enter 後的事件委派呼叫函示。</summary>
    /// <param name="command">輸入的指令字串。</param>
    private void OnCommandEntered(string command)
    {
        if (CommandEntered is null) return;
        foreach (CommandEnterHandler del in CommandEntered.GetInvocationList().Cast<CommandEnterHandler>())
            del.Invoke(this, command);
    }
    #endregion

    #region Private Method : bool OnBeforeMethodExecute(CommandAttribute cmd, MethodInfo method)
    /// <summary>指令連結的函示執行前的事件委派呼叫函示。</summary>
    /// <param name="cmd">執行的指令類別。</param>
    /// <param name="method">指令連結的函示。</param>
    /// <returns>事件回呼執行後是否繼續執行，繼續執行為 true, 否則為 false。</returns>
    private bool OnBeforeMethodExecute(CommandAttribute cmd, MethodInfo method)
    {
        ExecutingCommand = cmd;
        if (BeforeMethodExecute is null) return true;
        bool res = true;
        foreach (ExecuteCommandHandler del in BeforeMethodExecute.GetInvocationList().Cast<ExecuteCommandHandler>())
            res &= del.Invoke(this, cmd, method);
        return res;
    }
    #endregion

    #region Private Method : bool OnAfterMethodExecute(CommandAttribute cmd, MethodInfo method)
    /// <summary>指令連結的函示執行後的事件委派呼叫函示。</summary>
    /// <param name="cmd">執行的指令類別。</param>
    /// <param name="method">指令連結的函示。</param>
    /// <returns>事件回呼執行後是否繼續執行，繼續執行為 true, 否則為 false。</returns>
    private bool OnAfterMethodExecute(CommandAttribute cmd, MethodInfo method)
    {
        if (AfterMethodExecute is null)
        {
            ExecutingCommand = null;
            return true;
        }
        bool res = true;
        foreach (ExecuteCommandHandler del in AfterMethodExecute.GetInvocationList().Cast<ExecuteCommandHandler>())
            res &= del.Invoke(this, cmd, method);
        ExecutingCommand = null;
        return res;
    }
    #endregion


    #region Internal Static Method : void PrintCommands(IEnumerable<string> commands)
    /// <summary>顯示傳入的指令清單。</summary>
    /// <param name="commands">指令清單</param>
    internal static void PrintCommands(IEnumerable<string> commands)
    {
        int _m = commands.Max(_ca => _ca.Length);
        int _cols = (Console.WindowWidth - 4) / (_m + 5);
        int _i = 0;
        foreach (string s in commands)
        {
            Console.Write($"{s.PadRight(_m + 5)}");
            _i++;
            if (_i % _cols == 0)
            {
                Console.WriteLine();
                _i = 0;
            }
        }
        if (_i != 0) Console.WriteLine();
        Console.WriteLine();
    }
    #endregion


    #region Internal Method : void PrintCommandTree(IEnumerable<CommandAttribute> commands, int level = 0)
    /// <summary>顯示 Command 集合的階層結構。</summary>
    internal void PrintCommandTree()
    {
        Console.WriteLine($"\x1B[90m[DEBUG]\x1B[39m Command Tree:");
        foreach (CommandAttribute ca in _Commands.Where(_c => string.IsNullOrEmpty(_c.Parent)))
        {
            if (ca.Method is null)
                Console.WriteLine($" \x1B[92m{ca.Command.PadRight(42)}\x1B[39m");
            else
                Console.WriteLine($" \x1B[92m{ca.Command.PadRight(42)}\x1B[39m > \x1B[94m{ca.Method?.Name}\x1B[39m");
            _PrintCommandTree(ca);
        }
    }
    #endregion

    #region Private Method : void _PrintCommandTree(CommandAttribute cmd)
    private void _PrintCommandTree(CommandAttribute cmd)
    {
        foreach (CommandAttribute ca in cmd.Childs)
        {
            if (ca.IsRegular)
                Console.Write($" {"".PadLeft((ca.Level - 1) * 2)}+ \x1B[93m{ca.RegularHelp.PadRight(40 - (ca.Level - 1) * 2)}\x1B[39m");
            else
                Console.Write($" {"".PadLeft((ca.Level - 1) * 2)}+ \x1B[92m{ca.Command.PadRight(40 - (ca.Level - 1) * 2)}\x1B[39m");
            if (ca.Method is null)
                Console.WriteLine();
            else
                Console.WriteLine($" > \x1B[94m{ca.Method?.Name}\x1B[39m");
            _PrintCommandTree(ca);
        }
    }
    #endregion
}
#endregion
