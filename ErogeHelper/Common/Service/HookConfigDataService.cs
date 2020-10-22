using ErogeHelper.Model;

namespace ErogeHelper.Service
{
    class HookConfigDataService : IHookConfigDataService
    {
        public HookBindingList<long, HookParam> GetHookMapData()
        {
            return new HookBindingList<long, HookParam>(p => p.Handle);
        }
    }
}
