using System.Reflection;

namespace CJF.CommandLine;

#region Public Delegate Methods
/// <summary>定義自 <see cref="Reader.Read"/> 接收到指令的事件委派函示。</summary>
/// <param name="sender">發送來源，即為 <see cref="CliCenter"/> 執行個體。</param>
/// <param name="commandLine">完整的指令字串。</param>
public delegate void CommandEnterHandler(CliCenter sender, string commandLine);
/// <summary>定義執行的指令的事件委派函示。</summary>
/// <param name="sender">發送來源，即為 <see cref="CliCenter"/> 執行個體。</param>
/// <param name="cmd">觸發的指令類別屬性 <see cref="CommandAttribute"/>。</param>
/// <param name="method">對應 <paramref name="cmd"/> 指令的執行函示。</param>
/// <returns>事件回呼執行後是否繼續執行，繼續執行為 <see langword="true"/>, 否則為 <see langword="false"/>。</returns>
public delegate bool ExecuteCommandHandler(CliCenter sender, CommandAttribute cmd, MethodInfo method);
#endregion

#region Public Class : CommandException
/// <summary>定義指令重複的錯誤類別。</summary>
public class CommandException : Exception
{
    /// <summary>取得觸發錯誤的指令類別屬性 <see cref="CommandAttribute"/>。</summary>
    public CommandAttribute? Command { get; private set; } = null;

    /// <summary>建立新指令的錯誤類別。</summary>
    /// <param name="cmd">發生錯誤的指令類別。</param>
    public CommandException(CommandAttribute cmd)
    {
        Command = cmd;
    }
    /// <summary>建立新指令的錯誤類別。</summary>
    /// <param name="msg">錯誤訊息。</param>
    public CommandException(string msg) : base(msg)
    {
        Command = null;
    }
    // <summary>建立新指令的錯誤類別。</summary>
    /// <param name="cmd">發生錯誤的指令類別。</param>
    /// <param name="msg">錯誤訊息。</param>
    public CommandException(CommandAttribute cmd, string msg) : base(msg)
    {
        Command = cmd;
    }
}
#endregion

#region Public Sealed Class : CommandArgumentException
/// <summary>定義指令參數的錯誤類別。</summary>
public sealed class CommandArgumentException : CommandException
{
    /// <summary>建立新的指令參數錯誤類別。</summary>
    /// <param name="cmd">發生錯誤的指令類別。</param>
    public CommandArgumentException(CommandAttribute cmd) : base(cmd) { }
    /// <summary>建立新的指令參數錯誤類別。</summary>
    /// <param name="msg">錯誤訊息。</param>
    public CommandArgumentException(string msg) : base(msg) { }
}
#endregion

#region Public Sealed Class : CommandDuplicateException
/// <summary>定義指令重複的錯誤類別。</summary>
public sealed class CommandDuplicateException : CommandException
{
    /// <summary>取得觸發錯誤的指令執行函示。</summary>
    public MethodInfo? Method { get; private set; }

    /// <summary>建立新的指令重複的錯誤類別。</summary>
    /// <param name="msg">錯誤訊息。</param>
    public CommandDuplicateException(string msg) : base(msg) { }
    /// <summary>建立新的指令重複的錯誤類別。</summary>
    /// <param name="cmd">已存在定義的指令類別。</param>
    /// <param name="method">重複定義的執行函示。</param>
    public CommandDuplicateException(CommandAttribute cmd, MethodInfo method) : base(cmd)
    {
        Method = method;
    }
    /// <summary>建立新的指令重複的錯誤類別。</summary>
    /// <param name="cmd">已存在定義的指令類別。</param>
    /// <param name="method">重複定義的執行函示。</param>
    /// <param name="msg">錯誤訊息。</param>
    public CommandDuplicateException(CommandAttribute cmd, MethodInfo method, string msg) : base(cmd, msg)
    {
        Method = method;
    }
}
#endregion

#region Public Sealed Class : CommandRegularHelpEmptyException
/// <summary>定義正規表示式指令類型卻未指定其表示式說明文藝的錯誤類別。</summary>
public sealed class CommandRegularHelpEmptyException : CommandException
{
    /// <summary>尖利新的錯誤類別。</summary>
    /// <param name="msg">錯誤訊息。</param>
    public CommandRegularHelpEmptyException(string msg) : base(msg) { }
}
#endregion
