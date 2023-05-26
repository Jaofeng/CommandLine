namespace CJF.CommandLine.Abstractions
{
    internal interface IConsole
    {
        /// <summary>游標水平位置。</summary>
        int CursorLeft { get; }
        /// <summary>游標垂直位置。</summary>
        int CursorTop { get; }
        /// <summary>取得緩衝區的寬度(字元數)。</summary>
        int BufferWidth { get; }
        /// <summary>取得緩衝區的高度(字元數)。</summary>
        int BufferHeight { get; }
        /// <summary>取得或設定游標是否可見。</summary>
        bool CursorVisible { get; set; }
        /// <summary>設定游標位置。</summary>
        void SetCursorPosition(int left, int top);
        /// <summary>設定緩衝區的大小。</summary>
        void SetBufferSize(int width, int height);
        /// <summary>將字串印出。</summary>
        void Write(string? value);
        /// <summary>將字串印出，並加上分行符號。</summary>
        void WriteLine(string? value);
    }
}