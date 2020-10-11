using ErogeHelper.Model.Singleton;
using ErogeHelper.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ErogeHelper.Common
{
    static class Textractor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Textractor));

        private static readonly GameInfo gameInfo = GameInfo.Instance;

        private static string Path = null;

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
            catch (Exception ex)
            {
                log.Debug(ex.ToString());
                throw ex;
            }

            log.Info("initilize over.");

            textractor.OutputDataReceived += (sender, e) => {
                Dispatch(e.Data);
            };

            textractor.BeginOutputReadLine();
            foreach (Process p in gameInfo.ProcList)
            {
                textractor.StandardInput.WriteLine("attach -P" + p.Id);
                textractor.StandardInput.Flush();
                log.Info($"attach to PID {p.Id}.");
            }
        }

        // TODO: dettach none use thread

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
                // 提取匹配项内的分组信息
                hp.Handle = long.Parse(groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                hp.Pid = long.Parse(groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                hp.Addr = long.Parse(groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                hp.Ctx = long.Parse(groups[4].Value, System.Globalization.NumberStyles.HexNumber);
                hp.Ctx2 = long.Parse(groups[5].Value, System.Globalization.NumberStyles.HexNumber);
                hp.Name = groups[6].Value;
                hp.Hookcode = groups[7].Value;
                hp.Text = groups[8].Value;
            }

            DataEvent?.Invoke(typeof(Textractor), hp);

            if (gameInfo.HookCode != null && 
                gameInfo.HookCode == hp.Hookcode &&
                (gameInfo.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF))
            {
                switch (gameInfo.RepeatType)
                {
                    case "AABB":
                        patten = $@"([^\\]){{{gameInfo.RepeatTime}}}";
                        regex = new Regex(patten);
                        matches = regex.Matches(hp.Text);

                        if (matches.Count != 0)
                        {
                            string tmp = "";
                            foreach (Match match in matches)
                            {
                                tmp += match.Groups[1];
                            }
                            hp.Text = tmp;
                        }
                        break;
                    case "ABAB":
                        break;
                    default:
                        break;
                }
                log.Info(hp.Text);
                SelectedDataEvent?.Invoke(typeof(Textractor), hp);
            }
        }
    }
}
