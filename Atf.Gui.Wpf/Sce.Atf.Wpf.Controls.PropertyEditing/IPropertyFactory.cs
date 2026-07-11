using System.ComponentModel;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public interface IPropertyFactory
{
	PropertyNode CreateProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable, ITransactionContext context);
}
