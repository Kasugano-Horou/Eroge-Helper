using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ErogeHelper.Model.Singleton
{
    class GameInfo
    {
        private static readonly Lazy<GameInfo> lazy = new Lazy<GameInfo>(() => new GameInfo());

        public static GameInfo Instance { get { return lazy.Value; } }

        private GameInfo() { }

        #region 程序启动时读入的信息
        /// <summary>
        /// 绝对路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 文件夹绝对路径
        /// </summary>
        public string Dir { get; set; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string ProcessName { get; set; } 

        /// <summary>
        /// .eh.config 文件绝对路径
        /// </summary>
        public string ConfigPath { get; set; }

        public string MD5 { get; set; }
        #endregion

        #region 读取XML配置文件或选择钩子窗口时读入的信息
        public string HookCode { get; set; }

        public long ThreadContext { get; set; }

        /// <summary>
        /// 特殊码对应文本的重复类型；值为AABB ABAB NONE 三类
        /// </summary>
        public string RepeatType { get; internal set; }

        public int RepeatTime { get; internal set; }
        #endregion

        public List<Process> ProcList = new List<Process>(); // 考虑存在已经退出的进程
        public Process HWndProc { get; set; }
    }
}
