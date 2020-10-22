using ErogeHelper.Model;
using System.Collections.ObjectModel;

namespace ErogeHelper.Service
{
    public interface IGameDataService
    {
        ObservableCollection<SingleTextItem> InitTextData(TextTemplateType templateType);
    }
}
