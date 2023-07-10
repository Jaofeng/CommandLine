/*
 * File Name    : Reader.cs
 * From         : https://github.com/tonerdo/readline/blob/master/src/ReadLine/ReadLine.cs
 * Last Updated : Chen Jaofeng @ 2023/06/05
 */

using CJF.CommandLine.Abstractions;
using System.Runtime.Versioning;

namespace CJF.CommandLine;

#region Public Static Class : ReadLine
/// <summary>靜態類別 <see cref="Reader"/>。</summary>
[UnsupportedOSPlatform("browser")]
static class Reader
{
    private static readonly Dictionary<string, List<string>> _HistoryPool;
    private static bool _Stop = false;

    #region Private Static Construct Method : ReadLine()
    static Reader()
    {
        _HistoryPool = new Dictionary<string, List<string>>();
    }
    #endregion

    /// <summary>是否啟用歷史紀錄。</summary>
    public static bool HistoryEnabled { get; set; } = true;
    /// <summary>取得或設定當前歷史紀錄清單的名稱。</summary>
    internal static string PoolName { get; private set; } = "";
    /// <summary>設定或取得密碼輸入時的顯示字元。</summary>
    public static char? PasswordChar { get; set; }
    /// <summary>新增歷史指令。</summary>
    /// <param name="text">指令。</param>
    public static void AddHistory(params string[] text) => _HistoryPool[PoolName].AddRange(text);
    /// <summary>取得歷史指令清單。</summary>
    public static IEnumerable<string> GetHistory() => _HistoryPool[PoolName];
    /// <summary>清除當前歷史清單的紀錄。</summary>
    public static void ClearHistory() => _HistoryPool[PoolName] = new List<string>();
    /// <summary>取得或設定自動完成處理程序。</summary>
    public static Func<string, int, string[]?>? AutoCompletionHandler { private get; set; } = null;
    /// <summary>刪除最後一筆歷史紀錄。</summary>
    public static void RemoveLastHistory() => _HistoryPool[PoolName].RemoveAt(_HistoryPool[PoolName].Count - 1);

    #region Public Static Method : string Read(string prompt = "", ConsoleColor? promptColor = null, string @default = "")
    /// <summary>讀取使用者輸入的文字。</summary>
    /// <param name="prompt">提示文字。</param>
    /// <param name="promptColor">提示文字的顏色。</param>
    /// <param name="default">預設已輸入的文字。</param>
    /// <returns>使用者輸入的文字。</returns>
    public static string Read(string prompt = "", ConsoleColor? promptColor = null, string @default = "")
    {
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            if (promptColor is not null)
            {
                ConsoleColor cc = Console.ForegroundColor;
                Console.ForegroundColor = promptColor.Value;
                Console.Write(prompt);
                Console.ForegroundColor = cc;
            }
            else
                Console.Write(prompt);
        }
        _HistoryPool.TryGetValue(PoolName, out List<string>? pool);
        var keyHandler = new KeyHandler(new Console2(), pool, AutoCompletionHandler);
        _Stop = false;
        string text = GetText(keyHandler, @default);
        if (text.EndsWith('\x1B'))
            return Read(prompt, promptColor, text.TrimEnd('\x1B'));
        else
        {
            if (!string.IsNullOrWhiteSpace(text) && HistoryEnabled && _HistoryPool.ContainsKey(PoolName))
            {
                var exists = _HistoryPool[PoolName].Exists(x => x == text);
                // 2021/05/19 : 歷史紀錄為空、輸入的命令不在歷史紀錄內或者最後一筆紀錄不和輸入的命令相同
                if (_HistoryPool[PoolName].Count == 0 || !exists || _HistoryPool[PoolName].Last() != text)
                    _HistoryPool[PoolName].Add(text);
            }
        }

        return text;
    }
    #endregion

    #region Publuc Static Method : string ReadPassword(string prompt = "", char? pwdChar = null)
    /// <summary>讀取使用者輸入的密碼。</summary>
    /// <param name="prompt">提示文字。</param>
    /// <param name="pwdChar">顯示用的密碼字元。</param>
    /// <returns>使用者輸入的密碼。</returns>
    public static string ReadPassword(string prompt = "", char? pwdChar = null)
    {
        if (!string.IsNullOrWhiteSpace(prompt)) Console.Write(prompt);
        if (!pwdChar.HasValue)
            pwdChar = PasswordChar;
        KeyHandler keyHandler = new(new Console2() { PasswordMode = true, PasswordChar = pwdChar }, null, null);
        _Stop = false;
        return GetText(keyHandler);
    }
    #endregion

    #region Public Static Method : void ExitRead()
    /// <summary>結束讀取。</summary>
    public static void ExitRead()
    {
        _Stop = true;
    }
    #endregion

    #region Internal Static Method : void SetPool(string poolName)
    /// <summary>設定歷史紀錄清單的名稱。</summary>
    internal static void SetPool(string poolName)
    {
        PoolName = poolName;
        if (!_HistoryPool.ContainsKey(poolName))
            _HistoryPool.Add(poolName, new List<string>());
    }
    #endregion

    #region Private Method : string GetText(KeyHandler keyHandler, string @default = "")
    /// <summary>取得使用者輸入的文字。</summary>
    /// <param name="keyHandler">鍵盤處理程序。</param>
    /// <param name="default">預設已輸入的文字。</param>
    /// <returns>使用者輸入的文字。</returns>
    private static string GetText(KeyHandler keyHandler, string @default = "")
    {
        if (!string.IsNullOrEmpty(@default))
        {
            foreach (char c in @default)
                keyHandler.WriteChar(c);
        }
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        while (!_Stop && keyInfo.Key != ConsoleKey.Enter)
        {
            keyHandler.Handle(keyInfo);
            if (keyInfo.KeyChar == '?') break;
            else if (keyInfo.Key == ConsoleKey.Tab && keyHandler.IsMultiAutoCompleteResult())
                return keyHandler.Text + '\x1B';
            else
                keyInfo = Console.ReadKey(true);
        }

        Console.WriteLine();
        Console.CursorVisible = true;
        return keyHandler.Text;
    }
    #endregion
}
#endregion
