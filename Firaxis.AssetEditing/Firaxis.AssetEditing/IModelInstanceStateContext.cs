using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public interface IModelInstanceStateContext : IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ITreeListView, IItemView, ICommandClient, ISelectionContext
{
}
