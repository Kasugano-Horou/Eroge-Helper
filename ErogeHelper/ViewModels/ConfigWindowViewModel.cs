using ErogeHelper.Models;
using ErogeHelper.Utils;
using ErogeHelper.Views;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

namespace ErogeHelper.ViewModels
{
    class ConfigWindowViewModel : ObservableObject
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConfigWindowViewModel));

        public HookBindingList<long, HookParam> HookMapData { get; set; }
        public ConfigWindowViewModel()
        {
            HookMapData = new HookBindingList<long, HookParam>(p => p.handle);

            Textractor.DataEvent += DataRecvEventHandler;
        }

        private void DataRecvEventHandler(object sender, HookParam hp)
        {
            SynchronizationContext.SetSynchronizationContext(new
                System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
            SynchronizationContext.Current.Post(pl =>
            {
                var targetItem = HookMapData.FastFind(hp.handle);
                if (targetItem == null)
                {
                    HookMapData.Insert(0, hp);
                }
                else
                {
                    HookMapData.Remove(targetItem);
                    HookMapData.Insert(0, hp);
                }

            }, null);
        }

        public HookParam SelectedHook { get; set; }

        // This is where the Command binding points on the "Convert" button in the XAML
        // This uses the DelegateCommand class to encapsulate the ConvertText method (above) in a WPF Command
        public ICommand OKCommand
        {
            get
            {
                // More under the hood info about DelegateCommand can be found in DelegateCommand.cs, but this basically slides ConvertText in as the Execute() method of the command
                // If you're not familiar with commands, you should learn them, but for now just understand that whatever function you pass to DelegateCommand's constructor
                // (in this case, ConvertText) will run when the button data-bound to this command is pressed
                return new DelegateCommand(OpenGameWindow);
            }
        }

        private void OpenGameWindow()
        {
            if (SelectedHook != null)
            {
                log.Info($"Selected Hook: {SelectedHook.hookcode}");
                // find thread number of code
                // FIXME: baka
                List<long> allCtxForCode = new List<long>();
                foreach (HookParam hp in HookMapData)
                {
                    if (hp.hookcode == SelectedHook.hookcode)
                        allCtxForCode.Add(hp.ctx);
                }
                allCtxForCode.Sort();
                allCtxForCode.Reverse();

                var serial = 0;
                foreach ( long ctx in allCtxForCode)
                {
                    if (ctx == SelectedHook.ctx)
                        break;
                    else
                        serial++;
                }
                // write xml file
                try
                {
                    EHConfig.WriteConfig(GameInfo.Instance.ConfigPath, new EHProfile()
                    {
                        HCode = SelectedHook.hookcode,
                        MD5 = GameInfo.Instance.MD5,
                        HookThread = serial,
                        Name = GameInfo.Instance.ProcessName + ".eh.config"
                    });
                }
                catch (Exception)
                {
                    throw;
                }

                GameInfo.Instance.HookCode = SelectedHook.hookcode;
                GameInfo.Instance.HookThread = serial;
                new GameWindow().Show();
            }
        }
    }

    public class HookBindingList<TKey, TVal> : BindingList<TVal>
    {
        private readonly IDictionary<TKey, TVal> _dict = new Dictionary<TKey, TVal>();
        private readonly Func<TVal, TKey> _keyFunc;

        /// <summary>
        /// 构造函数, 匿名函数 p => p.xxx 给出索引类型
        /// </summary>
        /// <param name="keyFunc"></param>
        public HookBindingList(Func<TVal, TKey> keyFunc)
        {
            _keyFunc = keyFunc;
        }

        public HookBindingList(Func<TVal, TKey> keyFunc, IList<TVal> sourceList) : base(sourceList)
        {
            _keyFunc = keyFunc;

            foreach (var item in sourceList)
            {
                var key = _keyFunc(item);
                _dict.Add(key, item);
            }
        }

        /// <summary>
        /// 给出初始化时定义的 key，返回 TVal
        /// </summary>
        /// <param name="key"></param>
        /// <returns>TVal</returns>
        public TVal FastFind(TKey key)
        {
            TVal val;
            _dict.TryGetValue(key, out val);
            return val;
        }

        // -- 以下重写都不能直接调用，为底层方法 --

        protected override void InsertItem(int index, TVal val)
        {
            _dict.Add(_keyFunc(val), val);
            base.InsertItem(index, val);
        }

        protected override void SetItem(int index, TVal val)
        {
            var key = _keyFunc(val);
            _dict[key] = val;

            base.SetItem(index, val);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            var key = _keyFunc(item);
            _dict.Remove(key);

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            _dict.Clear();
            base.ClearItems();
        }
    }
}
