using ErogeHelper.Model;

namespace ErogeHelper.Service
{
    class DesignHookConfigDataService : IHookConfigDataService
    {
        public HookBindingList<long, HookParam> GetHookMapData()
        {
            HookBindingList<long, HookParam> ret = new HookBindingList<long, HookParam>(p => p.Handle);

            HookParam hp = new HookParam()
            {
                Addr = 0,
                Ctx = 0,
                Ctx2 = 0,
                Handle = 1,
                Hookcode = "e@e.exe",
                Name = "engine",
                Pid = 10000,
                Text = "Text is me"
            };
            ret.Add(hp);
            hp = new HookParam()
            {
                Addr = 1,
                Ctx = 1,
                Ctx2 = 1,
                Handle = 2,
                Hookcode = "e@e.exe",
                Name = "engine",
                Pid = 10000,
                Text = "second text"
            };
            ret.Add(hp);

            return ret;
        }
    }
}
