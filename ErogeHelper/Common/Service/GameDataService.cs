using ErogeHelper.Model;
using System.Collections.ObjectModel;

namespace ErogeHelper.Service
{
    class GameDataService : IGameDataService
    {
        public ObservableCollection<SingleTextItem> InitTextData(TextTemplateType templateType)
        {
            return new ObservableCollection<SingleTextItem>();
        }
    }
}
