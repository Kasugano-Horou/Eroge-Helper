using ErogeHelper.Model;

namespace ErogeHelper.Service
{
    public interface IHookConfigDataService
    {
        HookBindingList<long, HookParam> GetHookMapData();
    }
}
