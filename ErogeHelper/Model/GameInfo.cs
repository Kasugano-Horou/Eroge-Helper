using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model
{
    class GameInfo
    {
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

            public long SubThreadContext { get; set; }
            #endregion

            public List<Process> ProcList = new List<Process>(); // 考虑存在已经退出的进程
            public Process HWndProc { get; set; }
    }
}
