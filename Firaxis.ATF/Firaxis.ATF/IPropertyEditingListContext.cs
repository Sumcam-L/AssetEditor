using System.ComponentModel;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public interface IPropertyEditingListContext : IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext
{
	ListSortDirection DefaultListSortDirection { get; }

	string DefaultSortPropertyName { get; }
}
