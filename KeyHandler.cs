/*
 * File Name    : KeyHandler.cs
 * From         : https://github.com/tonerdo/readline/blob/master/src/ReadLine/KeyHandler.cs
 * Last Updated : Chen Jaofeng @ 2023/05/22
 */

using CJF.CommandLine.Abstractions;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace CJF.CommandLine;

#region Internal Class : KeyHandler
[UnsupportedOSPlatform("browser")]
internal class KeyHandler
{
    /// <summary>文字緩衝區。</summary>
    private readonly StringBuilder _TextBuffer;
    /// <summary>輸入列開始的水平游標位置。</summary>
    private readonly int _CursorStartPos;
    /// <summary>游標處於文字的位置。</summary>
    /// <remarks>
    /// <para>以字元(<see cref="char"/>)為單位，即使為雙寬度字元(如中文)亦為 1。</para>
    /// <para>如果位置在最後一位，其值等於 <see cref="_TextBuffer"/> 的長度。</para>
    /// </remarks>
    private int _CursorCharPos;
    /// <summary>游標在輸入區的位置，從 0 開始。</summary>
    /// <remarks>
    /// <para>以寬度為單位，英數字為 1，中文全形字為 2。</para>
    /// <para>當 <see cref="_TextBuffer"/> 長度為 0、或在行首，<see cref="_CursorPos"/> 為 0；</para>
    /// <para>如果在行尾，則 <see cref="_CursorPos"/> = <see cref="TextMaxWidth"/>。</para>
    /// </remarks>
    private int _CursorPos;
    /// <summary>歷史指令儲存區索引值。</summary>
    private int _HistoryIndex;
    /// <summary>歷史指令儲存區。</summary>
    private readonly List<string> _History;
    /// <summary><see cref="Console.ReadKey()"/> 讀到的按鍵值。</summary>
    private ConsoleKeyInfo _PressKey;
    /// <summary>控制類按鍵行為函示集合字典。</summary>
    private readonly Dictionary<string, Action> _KeyActions;
    /// <summary>自動完成的字串陣列。</summary>
    private string[]? _Completions;
    private int _CompletionStart;
    private int _CompletionsIndex;
    private readonly IConsole Console2;

    /// <summary>輸入的文字總寬度，英數字為 1，中文字為 2 。</summary>
    private int TextMaxWidth => _TextBuffer.ToString().GetWidth();
    /// <summary>是否在輸入列的首位。</summary>
    /// <returns>如果在行首則為 <see langword="true"/>，否則為 <see langword="false"/>。</returns>
    private bool IsStartOfLine() => _CursorPos == 0;
    /// <summary>是否在輸入列的末位。</summary>
    /// <returns>如果在行尾則為 <see langword="true"/>，否則為 <see langword="false"/>。</returns>
    private bool IsEndOfLine() => _CursorPos == TextMaxWidth;
    /// <summary>是否在該列的首位。</summary>
    private bool IsStartOfBuffer() => Console2.CursorLeft == 0;
    /// <summary>是否在該列的末位。</summary>
    private bool IsEndOfBuffer() => Console2.CursorLeft == Console2.BufferWidth - 1;
    /// <summary>是否在自動完成模式。</summary>
    private bool IsInAutoCompleteMode() => _Completions is not null;
    public static Action<string[]>? PrintCommandsHandler { private get; set; }
    public bool IsMultiAutoCompleteResult() => _Completions is not null && _Completions.Length > 1;
    /// <summary>取得輸入列的文字內容。</summary>
    public string Text => _TextBuffer.ToString();

    #region Private Method : void MoveCursorLeft(bool cursorVisible = true)
    private void MoveCursorLeft(bool cursorVisible = true)
    {
        if (IsStartOfLine()) return;
        if (cursorVisible) Console2.CursorVisible = false;

        if (IsStartOfBuffer() && _TextBuffer.Length > 0)
        {
            Console2.SetCursorPosition(Console2.BufferWidth - 1, Console2.CursorTop - 1);
            _CursorPos = Console2.CursorLeft;
        }
        else if (_CursorCharPos > 0 && _TextBuffer.Length > 0)
        {
            int charLen = _TextBuffer[_CursorCharPos - 1].IsDoubleWord() ? 2 : 1;
            Console2.SetCursorPosition(Console2.CursorLeft - charLen, Console2.CursorTop);
            _CursorPos -= charLen;
        }
        else
            return;
        if (cursorVisible) Console2.CursorVisible = true;
        _CursorCharPos--;
        Debug.Print($"CursorPos:{_CursorPos}, CursorLimit:{TextMaxWidth}, TextPos:{_CursorCharPos}");
    }
    #endregion

    #region Private Method : void MoveCursorHome(bool cursorVisible = true)
    private void MoveCursorHome(bool cursorVisible = true)
    {
        if (cursorVisible) Console2.CursorVisible = false;
        //Console2.SetCursorPosition(_CursorStartPos, Console2.CursorTop);
        // 避免游標定位在指令行時，被新訊息給更新而造成定位錯誤，所以使用相對位置定位
        Console2.SetCursorPosition(Console2.CursorLeft - _CursorCharPos, Console2.CursorTop);
        _CursorPos = 0;
        _CursorCharPos = 0;
        if (cursorVisible) Console2.CursorVisible = true;
    }
    #endregion

    #region Private Method : void MoveCursorRight(bool cursorVisible = true)
    private void MoveCursorRight(bool cursorVisible = true)
    {
        if (IsEndOfLine()) return;
        if (cursorVisible) Console2.CursorVisible = false;
        if (IsEndOfBuffer())
        {
            Console2.SetCursorPosition(0, Console2.CursorTop + 1);
            _CursorPos = Console2.CursorLeft;
        }
        else if (_TextBuffer.Length > _CursorCharPos)
        {
            int charLen = _TextBuffer[_CursorCharPos].IsDoubleWord() ? 2 : 1;
            Console2.SetCursorPosition(Console2.CursorLeft + charLen, Console2.CursorTop);
            _CursorPos += charLen;
        }
        if (cursorVisible) Console2.CursorVisible = true;
        _CursorCharPos++;
        Debug.Print($"CursorPos:{_CursorPos}, CursorLimit:{TextMaxWidth}, TextPos:{_CursorCharPos}");
    }
    #endregion

    #region Private Method : void MoveCursorEnd(bool cursorVisible = false)
    private void MoveCursorEnd(bool cursorVisible = false)
    {
        if (cursorVisible) Console2.CursorVisible = false;
        //Console2.SetCursorPosition(_CursorStartPos + TextMaxWidth, Console2.CursorTop);
        // 避免游標定位在指令行時，被新訊息給更新而造成定位錯誤，所以使用相對位置定位
        Console2.SetCursorPosition(Console2.CursorLeft + (TextMaxWidth - _CursorCharPos), Console2.CursorTop);
        _CursorPos = TextMaxWidth;
        _CursorCharPos = _TextBuffer.Length;
        if (cursorVisible) Console2.CursorVisible = true;
    }
    #endregion

    #region Private Method : void ClearLine(bool cursorVisible = true)
    private void ClearLine(bool cursorVisible = true)
    {
        if (cursorVisible)
            Console2.CursorVisible = false;
        // 避免游標定位在指令行時，被新訊息給更新而造成定位錯誤，所以使用相對位置定位
        //Console2.SetCursorPosition(_CursorStartPos, Console2.CursorTop);
        Console2.SetCursorPosition(Console2.CursorLeft - _CursorCharPos, Console2.CursorTop);
        Console2.Write("".PadLeft(TextMaxWidth));
        //Console2.SetCursorPosition(_CursorStartPos, Console2.CursorTop);
        Console2.SetCursorPosition(Console2.CursorLeft - TextMaxWidth, Console2.CursorTop);
        _TextBuffer.Clear();
        _CursorPos = 0;
        _CursorCharPos = 0;
        if (cursorVisible)
            Console2.CursorVisible = true;
    }
    #endregion

    #region Private Method : void Backspace(bool cursorVisible = true)
    private void Backspace(bool cursorVisible = true)
    {
        if (IsStartOfLine() || _TextBuffer.Length == 0 || _CursorCharPos == 0) return;

        _CursorCharPos--;
        int index = _CursorCharPos;
        var c = _TextBuffer[index];
        var cLen = c.IsDoubleWord() ? 2 : 1;
        _TextBuffer.Remove(index, 1);
        string lastChars = _TextBuffer.ToString()[index..];
        int left = Console2.CursorLeft - cLen;
        int top = Console2.CursorTop;
        if (cursorVisible) Console2.CursorVisible = false;
        Console2.SetCursorPosition(left, top);
        Console2.Write($"{lastChars}  ");
        Console2.SetCursorPosition(left, top);
        if (cursorVisible) Console2.CursorVisible = true;
        _CursorPos -= cLen;
        Debug.Print($"CursorPos:{_CursorPos}, CursorLimit:{TextMaxWidth}, TextPos:{_CursorCharPos}");
    }
    #endregion

    #region Private Method : void Delete(bool cursorVisible = true)
    private void Delete(bool cursorVisible = true)
    {
        if (IsEndOfLine() || _TextBuffer.Length == 0) return;

        int index = _CursorCharPos;
        var c = _TextBuffer[index];
        _TextBuffer.Remove(index, 1);
        string lastChars = _TextBuffer.ToString()[index..];
        int left = Console2.CursorLeft;
        int top = Console2.CursorTop;
        if (cursorVisible) Console2.CursorVisible = false;
        Console2.Write($"{lastChars}  ");
        Console2.SetCursorPosition(left, top);
        if (cursorVisible) Console2.CursorVisible = true;
        Debug.Print($"CursorPos:{_CursorPos}, CursorLimit:{TextMaxWidth}, TextPos:{_CursorCharPos}");
    }
    #endregion

    #region Private Method : void WriteNewString(string str)
    private void WriteNewString(string str)
    {
        Console2.CursorVisible = false;
        ClearLine(false);
        foreach (char character in str)
            WriteChar(character, false);
        Console2.CursorVisible = true;
    }
    #endregion

    #region Private Method : void WriteString(string str)
    private void WriteString(string str)
    {
        Console2.CursorVisible = false;
        foreach (char character in str)
            WriteChar(character, false);
        Console2.CursorVisible = true;
    }
    #endregion

    #region Private Method : string BuildKeyInput()
    private string BuildKeyInput()
    {
        return (_PressKey.Modifiers != ConsoleModifiers.Control && _PressKey.Modifiers != ConsoleModifiers.Shift) ?
            _PressKey.Key.ToString() : _PressKey.Modifiers.ToString() + _PressKey.Key.ToString();
    }
    #endregion

    #region Private Method : void TransposeChars()
    private void TransposeChars()
    {
        // local helper functions
        bool almostEndOfLine() => (TextMaxWidth - _CursorPos) == 1;
        int incrementIf(Func<bool> expression, int index) => expression() ? index + 1 : index;
        int decrementIf(Func<bool> expression, int index) => expression() ? index - 1 : index;

        if (IsStartOfLine()) { return; }

        var firstIdx = decrementIf(IsEndOfLine, _CursorPos - 1);
        var secondIdx = decrementIf(IsEndOfLine, _CursorPos);

        //char secondChar = _TextBuffer[secondIdx];
        //_TextBuffer[secondIdx] = _TextBuffer[firstIdx];
        //_TextBuffer[firstIdx] = secondChar;
        (_TextBuffer[firstIdx], _TextBuffer[secondIdx]) = (_TextBuffer[secondIdx], _TextBuffer[firstIdx]);

        var left = incrementIf(almostEndOfLine, Console2.CursorLeft);
        var cursorPosition = incrementIf(almostEndOfLine, _CursorPos);

        WriteNewString(_TextBuffer.ToString());

        Console2.SetCursorPosition(left, Console2.CursorTop);
        _CursorPos = cursorPosition;

        MoveCursorRight();
    }
    #endregion

    #region Private Method : void StartAutoComplete()
    private void StartAutoComplete()
    {
        if (_Completions is null) return;
        Console2.CursorVisible = false;
        while (_CursorPos > _CompletionStart)
            Backspace(false);

        _CompletionsIndex = 0;
        WriteString(_Completions[_CompletionsIndex] + ' ');

    }
    #endregion

    #region Private Method : void NextAutoComplete()
    private void NextAutoComplete()
    {
        if (_Completions is null) return;
        while (_CursorPos > _CompletionStart)
            Backspace();

        _CompletionsIndex++;

        if (_CompletionsIndex == _Completions.Length)
            _CompletionsIndex = 0;

        WriteString(_Completions[_CompletionsIndex]);
    }
    #endregion

    #region Private Method : void PreviousAutoComplete()
    private void PreviousAutoComplete()
    {
        if (_Completions is null) return;
        while (_CursorPos > _CompletionStart)
            Backspace();

        _CompletionsIndex--;

        if (_CompletionsIndex == -1)
            _CompletionsIndex = _Completions.Length - 1;

        WriteString(_Completions[_CompletionsIndex]);
    }
    #endregion

    #region Private Method : void PrevHistory()
    private void PrevHistory()
    {
        if (_HistoryIndex > 0)
        {
            _HistoryIndex--;
            WriteNewString(_History[_HistoryIndex]);
        }
    }
    #endregion

    #region Private Method : void NextHistory()
    private void NextHistory()
    {
        if (_HistoryIndex < _History.Count)
        {
            _HistoryIndex++;
            if (_HistoryIndex == _History.Count)
                ClearLine();
            else
                WriteNewString(_History[_HistoryIndex]);
        }
    }
    #endregion

    #region Private Method : void ResetAutoComplete()
    private void ResetAutoComplete()
    {
        _Completions = null;
        _CompletionsIndex = 0;
    }
    #endregion


    #region Public Construct Method : KeyHandler(IConsole console, List<string>? history = null, Func<string, int, string[]?>? handler = null)
    public KeyHandler(IConsole console, List<string>? history = null, Func<string, int, string[]?>? handler = null)
    {
        Console2 = console;
        _CursorStartPos = Console2.CursorLeft;
        char[] separators = new char[] { ' ' };
        _History = history ?? new List<string>();
        _HistoryIndex = _History.Count;
        _TextBuffer = new StringBuilder();
        _KeyActions = new Dictionary<string, Action>
        {
            ["Escape"] = () => ClearLine(),
            ["Backspace"] = () => Backspace(),
            ["Delete"] = () => Delete(),
            ["LeftArrow"] = () => MoveCursorLeft(),
            ["RightArrow"] = () => MoveCursorRight(),
            ["Home"] = () => MoveCursorHome(),
            ["End"] = () => MoveCursorEnd(),
            ["UpArrow"] = () => PrevHistory(),
            ["ControlA"] = () => MoveCursorHome(),
            ["ControlB"] = () => MoveCursorLeft(),
            ["ControlD"] = () => Delete(),
            ["ControlE"] = () => MoveCursorEnd(),
            ["ControlF"] = () => MoveCursorRight(),
            ["ControlH"] = () => Backspace(),
            ["ControlL"] = () => ClearLine(),
            ["ControlP"] = () => PrevHistory(),
            ["DownArrow"] = () => NextHistory(),
            ["ControlN"] = () => NextHistory(),
            ["ControlU"] = () =>    // 刪除到行首的字元
            {
                while (!IsStartOfLine())
                    Backspace();
            },
            ["ControlK"] = () =>    // 刪除到行尾的字元
            {
                int pos = _CursorPos;
                MoveCursorEnd();
                while (_CursorPos > pos)
                    Backspace();
            },
            ["ControlW"] = () =>    // 刪除游標前的單字
            {
                while (!IsStartOfLine() && _TextBuffer[_CursorCharPos - 1] != ' ')
                    Backspace();
            },
            ["ControlT"] = TransposeChars,
            ["Tab"] = () =>
            {
                if (handler == null || !IsEndOfLine() || _TextBuffer.Length == 0) return;
                string text = _TextBuffer.ToString();

                _CompletionStart = text.LastIndexOfAny(separators);
                _CompletionStart = _CompletionStart == -1 ? 0 : _CompletionStart + 1;
                if (text.EndsWith(" ")) return;

                _Completions = handler.Invoke(text, _CompletionStart);
                _Completions = _Completions?.Length == 0 ? null : _Completions;
                if (_Completions == null) return;

                if (_Completions.Length >= 2)
                {
                    Console.Beep();
                    Console.WriteLine();
                    if (PrintCommandsHandler is not null)
                        PrintCommandsHandler.Invoke(_Completions);
                    else
                        CliCenter.PrintCommands(_Completions);
                }
                else
                    StartAutoComplete();
            },
        };
    }
    #endregion


    #region Public Method : void Handle(ConsoleKeyInfo keyInfo)
    public void Handle(ConsoleKeyInfo keyInfo)
    {
        _PressKey = keyInfo;
        if (IsInAutoCompleteMode() && _PressKey.Key != ConsoleKey.Tab)
            ResetAutoComplete();

        if (!_KeyActions.TryGetValue(BuildKeyInput(), out Action? action))
            action = () => WriteChar(_PressKey.KeyChar);
        action.Invoke();
    }
    #endregion

    #region Public Method : void WriteChar(char c, bool cursorVisible = true)
    public void WriteChar(char c, bool cursorVisible = true)
    {
        int charLen = c.IsDoubleWord() ? 2 : 1;
        string last = "";
        if (cursorVisible) Console2.CursorVisible = false;
        if (IsEndOfLine())
        {
            _TextBuffer.Append(c);
            Console2.Write(c.ToString());
        }
        else
        {
            int left = Console2.CursorLeft;
            int top = Console2.CursorTop;
            if (_CursorCharPos >= _TextBuffer.Length)
            {
                _TextBuffer.Append(c);
                _CursorCharPos = _TextBuffer.Length - 1;
            }
            else
            {
                _TextBuffer.Insert(_CursorCharPos, c);
                last = _TextBuffer.ToString()[_CursorCharPos..];
                Console2.CursorVisible = false;
                Console2.Write(last);
                Console2.SetCursorPosition(left + charLen, top);
            }
        }
        if (cursorVisible) Console2.CursorVisible = true;
        _CursorCharPos++;
        _CursorPos += charLen;
        Debug.Print($"CursorPos:{_CursorPos}, CursorLimit:{TextMaxWidth}, TextPos:{_CursorCharPos}, Last:{last}");
    }
    #endregion

}
#endregion

#region Static Class : StringExtensions
static class StringExtensions
{
    /// <summary>取得字串寬度。</summary>
    /// <param name="source">欲取得寬度的原始字串。</param>
    /// <returns>字串寬度。</returns>
    public static int GetWidth(this string source)
    {
        int width = 0;
        foreach (char c in source)
            width += c.IsDoubleWord() ? 2 : 1;
        return width;
    }
}
#endregion

#region Static Class : CharExtensions
static class CharExtensions
{
    /// <summary>判斷 <paramref name="c"/> 是否為雙位元組字元。</summary>
    /// <param name="c"></param>
    /// <returns>如果 <paramref name="c"/> 為雙位元組字元，則傳回 <see langword="true"/>，否則為 <see langword="false"/>。</returns>
    public static bool IsDoubleWord(this char c) => Encoding.UTF8.GetByteCount(c.ToString()) != 1;
}
#endregion