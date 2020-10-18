using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ErogeHelper.Model
{
    public class HookBindingList<TKey, TVal> : BindingList<TVal>
    {
        private readonly IDictionary<TKey, TVal> _dict = new Dictionary<TKey, TVal>();
        private readonly Func<TVal, TKey> _keyFunc;

        /// <summary>
        /// 构造函数, 匿名函数 p => p.xxx 给出TVal返回TKey
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
            _dict.TryGetValue(key, out TVal val);
            return val;
        }

        // --  --

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
