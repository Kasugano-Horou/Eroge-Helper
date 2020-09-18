using System;
using System.Diagnostics;

namespace ErogeHelper.Models
{
    /// <summary>
    /// Singleton, help to get things when I need
    /// </summary>
    class GameInfo
    {
        private static readonly Lazy<GameInfo> lazy = new Lazy<GameInfo>(() => new GameInfo());

        public static GameInfo Instance { get { return lazy.Value; } }

        private GameInfo() { }

        public string Path { get; set; }
        public string Dir { get; set; }
        public string ProcessName { get; set; } // aka friendly name
        public string ConfigPath { get; set; }

        public string MD5 { get; set; }
        public string HookCode { get; set; }
        public int HookThread { get; set; }

        public Process[] ProcList { get; set; } // 可能有些已经退出的进程
        public IntPtr hWnd { get; set; }

        public RECT Rect = new RECT(); // 存放游戏窗口坐标信息
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
