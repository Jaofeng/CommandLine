namespace CJF.CommandLine.Abstractions;

internal sealed class Console2 : IConsole
{
    public int CursorLeft => Console.CursorLeft;
    public int CursorTop => Console.CursorTop;
    public int BufferWidth => Console.BufferWidth;
    public int BufferHeight => Console.BufferHeight;
    public bool CursorVisible
    {
        get => !OperatingSystem.IsWindows() || Console.CursorVisible;
        set => Console.CursorVisible = value;
    }
    public bool PasswordMode { get; set; } = false;
    public char? PasswordChar { get; set; }

    public void SetBufferSize(int width, int height)
    {
        if (OperatingSystem.IsWindows())
            Console.SetBufferSize(width, height);
        else
            throw new NotSupportedException("OS is not support!");
    }

    public void SetCursorPosition(int left, int top)
    {
        left = Math.Max(0, left);
        top = Math.Max(0, top);
        if (!PasswordMode)
            Console.SetCursorPosition(left, top);
    }

    public void Write(string? value)
    {
        if (PasswordMode && value is not null)
        {
            if (PasswordChar.HasValue)
                value = "".PadRight(value.Length, PasswordChar.Value);
            else
                value = "";
        }
        Console.Write(value);
    }
    public void WriteLine(string? value) => Console.WriteLine(value);
}