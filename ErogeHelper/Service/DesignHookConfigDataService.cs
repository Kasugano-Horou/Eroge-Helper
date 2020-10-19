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
                Handle = 0,
                Hookcode = "e@e.exe",
                Name = "engine",
                Pid = 10000,
                Text = "Text is me",
                TotalText = "1first text\n" +
                            "2\n" +
                            "3\n" +
                            "4\n" +
                            "5\n" +
                            "6\n" +
                            "7\n" +
                            "8\n" +
                            "9\n" +
                            "10\n"
            };
            ret.Add(hp);
            hp = new HookParam()
            {
                Addr = 1,
                Ctx = 1,
                Ctx2 = 1,
                Handle = 1,
                Hookcode = "e@e.exe",
                Name = "engine",
                Pid = 10000,
                Text = "second text",
                TotalText = "second text"
            };
            ret.Add(hp);

            return ret;
        }
    }
}
