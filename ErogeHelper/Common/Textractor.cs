using ErogeHelper.Model;
using GalaSoft.MvvmLight.Ioc;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static ErogeHelper.Common.Textractor.TextHostLib;

namespace ErogeHelper.Common
{
    static class Textractor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Textractor));

        private static readonly GameInfo gameInfo = (GameInfo)SimpleIoc.Default.GetInstance(typeof(GameInfo));

        public static void Init()
        {
            log.Info("initilize start.");
            log.Info($"Work directory {Directory.GetCurrentDirectory()}");

            createthread = CreateThreadHandle;
            output = OutputHandle;
            removethread = RemoveThreadHandle;
            callback = OnConnectCallBackHandle;

            TextHostInit(callback, (_) => { }, createthread, removethread, output);

            foreach (Process p in gameInfo.ProcList)
            {
                InjectProcess((uint)p.Id);
                log.Info($"attach to PID {p.Id}.");
            }
            log.Info("initilize over.");
        }
        static private OnOutputText output;
        static private ProcessCallback callback;
        static private OnCreateThread createthread;
        static private OnRemoveThread removethread;

        public delegate void DataRecvEventHandler(object sender, HookParam e);
        public static event DataRecvEventHandler SelectedDataEvent;
        public static event DataRecvEventHandler DataEvent;

        static Dictionary<long, HookParam> ThreadHandleDict = new Dictionary<long, HookParam>();

        #region TextHostInit Callback Implement
        static public void CreateThreadHandle(
            long threadId,
            uint processId,
            ulong address,
            ulong context,
            ulong subcontext,
            string name,
            string hookCode)
        {
            ThreadHandleDict[threadId] = new HookParam
            {
                Handle = threadId,
                Pid = processId,
                Addr = (long)address,
                Ctx = (long)context,
                Ctx2 = (long)subcontext,
                Name = name,
                Hookcode = hookCode
            };
        }

        static public void OutputHandle(long threadid, string opdata)
        {
            HookParam hp = ThreadHandleDict[threadid];
            hp.Text = opdata;

            DataEvent?.Invoke(typeof(Textractor), hp);

            if (gameInfo.HookCode != null
                && gameInfo.HookCode == hp.Hookcode
                && (gameInfo.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                && gameInfo.SubThreadContext == hp.Ctx2)
            {
                log.Info(hp.Text);
                SelectedDataEvent?.Invoke(typeof(Textractor), hp);
            }
        }

        static public void RemoveThreadHandle(long threadId) { }

        static public void OnConnectCallBackHandle(uint processId)
        {
            if (File.Exists(gameInfo.ConfigPath))
            {
                InsertHook(gameInfo.HookCode);
            }
        }
        #endregion

        public static void InsertHook(string hookcode)
        {
            foreach (Process p in gameInfo.ProcList)
            {
                TextHostLib.InsertHook((uint)p.Id, hookcode);
                log.Info($"Try insert code {hookcode} to PID {p.Id}");
            }
        }

        internal class TextHostLib
        {
            #region 回调委托
            internal delegate void ProcessCallback(uint processId);

            internal delegate void OnCreateThread(
                long threadId,
                uint processId,
                ulong address,
                ulong context,
                ulong subcontext,
                [MarshalAs(UnmanagedType.LPWStr)] string name,
                [MarshalAs(UnmanagedType.LPWStr)] string hookCode
                );

            internal delegate void OnRemoveThread(long threadId);

            internal delegate void OnOutputText(long threadId, [MarshalAs(UnmanagedType.LPWStr)] string text);
            #endregion

            [DllImport(@"libs\texthost.dll")]
            internal static extern int TextHostInit(
                ProcessCallback OnConnect,
                ProcessCallback OnDisconnect,
                OnCreateThread OnCreateThread,
                OnRemoveThread OnRemoveThread,
                OnOutputText OnOutputText
                );

            [DllImport(@"libs\texthost.dll")]
            internal static extern int InsertHook(
                uint processId,
                [MarshalAs(UnmanagedType.LPWStr)] string hookCode
                );

            [DllImport(@"libs\texthost.dll")]
            internal static extern int RemoveHook(uint processId, ulong address);

            [DllImport(@"libs\texthost.dll")]
            internal extern static int InjectProcess(uint processId);

            [DllImport(@"libs\texthost.dll")]
            internal extern static int DetachProcess(uint processId);

            [DllImport(@"libs\texthost.dll")]
            internal extern static int AddClipboardThread(IntPtr windowHandle);

            //用于搜索钩子的结构体参数，32bit size=608 ,64bit size=632
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct SearchParam
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
                public byte[] pattern;

                public int Length;
                public int Offset;
                public int SearchTime;
                public int MaxRecords;
                public int Codepage;

                [MarshalAs(UnmanagedType.SysUInt)]
                public IntPtr Padding;

                [MarshalAs(UnmanagedType.SysUInt)]
                public IntPtr MinAddress;

                [MarshalAs(UnmanagedType.SysUInt)]
                public IntPtr MaxAddress;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
                public string BoundaryModule;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
                public string ExportModule;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
                public string Text;

                public IntPtr HookPostProcessor;
            };
        }
    }
}
