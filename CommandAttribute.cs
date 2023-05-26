using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Versioning;

namespace CJF.CommandLine;

#region Public Sealed Class : CommandAttribute
/// <summary>CLI 指令類別</summary>
[UnsupportedOSPlatform("browser")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false), DefaultProperty("Command")]
public sealed class CommandAttribute : Attribute
{
    readonly List<CommandAttribute> _Childs;

    #region Public Properties
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
    public CommandAttribute[] Childs => _Childs.ToArray();
    /// <summary>完整指令字串。</summary>
    public string FullCommand { get; private set; }
    /// <summary>指令執行的方法定義。</summary>
    public MethodInfo? Method { get; internal set; } = null;
    #endregion

    #region Public Construct Method : CommandAttribute(string command, string helpText, string? parent = null)
    /// <summary>建立新的 <see cref="CommandAttribute"/> 實體類別。</summary>
    /// <param name="command">CLI 指令，可使用正規表示式。</param>
    /// <param name="helpText">指令說明文字。</param>
    /// <param name="parent">上一層的完整指令。</param>
    public CommandAttribute(string command, string helpText, string? parent = null)
    {
        Command = command;
        HelpText = helpText;
        Parent = parent;
        FullCommand = Command;
        if (!string.IsNullOrEmpty(parent))
            FullCommand = $"{parent} {Command}";
        _Childs = new List<CommandAttribute>();
    }
    #endregion

    #region Internal Method : void AddChild(CommandAttribute cmd)
    /// <summary>新增子指令。</summary>
    /// <param name="cmd">子指令。</param>
    internal void AddChild(CommandAttribute cmd) => _Childs.Add(cmd);
    #endregion

    #region Internal Method : void AddChilds(IEnumerable<CommandAttribute> cmds)
    /// <summary>新增一個以上的子指令。</summary>
    /// <param name="cmds">子指令清單。</param>
    internal void AddChilds(IEnumerable<CommandAttribute> cmds) => _Childs.AddRange(cmds);
    #endregion

    #region Public Method : bool IsValid()
    /// <summary>檢驗此 <see cref="CommandAttribute"/> 是否有效。</summary>
    /// <returns>有效: <see langword="true"/>, 否則為 <see langword="false"/>。</returns>
    public bool IsValid() => !string.IsNullOrEmpty(Command) && !Command.Contains(' ') && (string.IsNullOrEmpty(Parent) || Parent is not null && !Parent.EndsWith(" "));
    #endregion


    #region Public Override Method : bool Match(object? obj)
    /// <summary>判斷指定的物件是否等於目前的 <see cref="CommandAttribute"/>。</summary>
    /// <param name="obj">指定的物件。</param>
    public override bool Match(object? obj)
    {
        if (obj is not CommandAttribute ca) return false;
        if (Object.ReferenceEquals(this, ca)) return true;
        return GetHashCode() == ca.GetHashCode();
    }
    #endregion

    #region Public Override Method : bool Equals([NotNullWhen(true)] object? obj)
    /// <summary>判斷指定的物件是否等於目前的 <see cref="CommandAttribute"/>。</summary>
    /// <param name="obj">指定的物件。</param>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return Match(obj);
    }
    #endregion

    #region Public Override Method : int GetHashCode()
    /// <summary>做為特定型別的雜湊函式。</summary>
    public override int GetHashCode()
    {
        if (Tag is not null)
            return FullCommand.GetHashCode() ^ Tag.GetHashCode() ^ Hidden.GetHashCode() ^ Required.GetHashCode() ^ HelpText.GetHashCode();
        else
            return FullCommand.GetHashCode() ^ Hidden.GetHashCode() ^ Required.GetHashCode() ^ HelpText.GetHashCode();
    }
    #endregion

    #region Public Override Method : string ToString()
    /// <summary>傳回代表 <see cref="CommandAttribute"/> 物件的字串。</summary>
    /// <returns>代表 <see cref="CommandAttribute"/> 物件的字串。</returns>
    public override string ToString()
    {
        string res = $"{{ Command:\"{Command}\", Parent:\"{Parent}\", HelpText:\"{HelpText}\", Tag:\"{Tag}\", FullCommand:\"{FullCommand}\", IsRegular:{IsRegular}, ";
        if (IsRegular)
            res += $"RegularHelp:\"{RegularHelp}\", ";
        res += $"Required:{Required}, Hidden:{Hidden}, ";
        res += $"Childs:[";
        foreach (CommandAttribute cmd in _Childs)
            res += cmd.ToString() + ", ";
        res = res.TrimEnd(',', ' ');
        res += $"], Valid: {IsValid()} }}";
        return res;
    }
    #endregion

    #region Public Static Operator Methods : == & !=
    /// <summary>判斷兩個 <see cref="CommandAttribute"/> 是否相等。</summary>
    public static bool operator ==(CommandAttribute left, CommandAttribute right) => left.Match(right);
    /// <summary>判斷兩個 <see cref="CommandAttribute"/> 是否不相等。</summary>
    public static bool operator !=(CommandAttribute left, CommandAttribute right) => !left.Match(right);
    #endregion
}
#endregion
