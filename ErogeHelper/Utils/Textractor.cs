using ErogeHelper.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ErogeHelper.Utils
{
    static class Textractor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Textractor));

        private static string Path = null;

        private static List<long> ThreadIndex = new List<long>();// Map serial number to ctx in one HCode

        /// <summary>
        /// Initlize textractor module, Only call once
        /// </summary>
        public static void Init()
        {
            log.Info("initilize start.");

            Path = Directory.GetCurrentDirectory();
            Path += @"\libs\x86\TextractorCLI.exe";

            log.Info($"Path: {Path}");
            ProcessStartInfo textractorInfo = new ProcessStartInfo()
            {
                FileName = Path,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.Unicode,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };

            Process textractor = new Process();
            try
            {
                textractor = Process.Start(textractorInfo);
            }
            catch(Exception ex)
            {
                log.Debug(ex.ToString());
                throw ex;
            }

            log.Info("initilize over.");

            textractor.OutputDataReceived += (sender, e) => {
                Dispatch(e.Data);
            };

            textractor.BeginOutputReadLine();
            foreach (Process p in GameInfo.Instance.ProcList)
            {
                textractor.StandardInput.WriteLine("attach -P" + p.Id);
                textractor.StandardInput.Flush();
            }

            log.Info($"attach to PID {GameInfo.Instance.ProcList[0].Id}.");

            // TODO: dettach none use thread
        }

        public delegate void DataRecvEventHandler(object sender, HookParam e);
        public static event DataRecvEventHandler SelectedDataEvent;
        public static event DataRecvEventHandler DataEvent;

        private static void Dispatch(string raw)
        {
            HookParam hp = new HookParam();

            string patten = @"\[(\w+):(\w+):(\w+):(\w+):(\w+):(\w+):(.+)] (.*)";
            Regex regex = new Regex(patten);
            MatchCollection matches = regex.Matches(raw);
            if (matches.Count == 1)
            {
                Match match = matches[0];
                GroupCollection groups = match.Groups;
                //提取匹配项内的分组信息
                hp.handle = Int64.Parse(groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                hp.pid = Int64.Parse(groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                hp.addr = Int64.Parse(groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                hp.ctx = Int64.Parse(groups[4].Value, System.Globalization.NumberStyles.HexNumber);
                hp.ctx2 = Int64.Parse(groups[5].Value, System.Globalization.NumberStyles.HexNumber);
                hp.name = groups[6].Value;
                hp.hookcode = groups[7].Value;
                hp.text = groups[8].Value;
            }

            DataEvent?.Invoke(typeof(Textractor), hp);

            if (GameInfo.Instance.HookCode == hp.hookcode)
            {
                if (!ThreadIndex.Contains(hp.ctx))
                {
                    ThreadIndex.Add(hp.ctx);
                    ThreadIndex.Sort();
                    ThreadIndex.Reverse();
                }
               
                // HookThread越小 - ctx 越大 - 越晚被捕捉 - 在Config页面的选择越靠前
                if (ThreadIndex[GameInfo.Instance.HookThread] == hp.ctx) 
                {
                    log.Info(hp.text);
                    SelectedDataEvent?.Invoke(typeof(Textractor), hp);
                }
            }
        }
    }
}
