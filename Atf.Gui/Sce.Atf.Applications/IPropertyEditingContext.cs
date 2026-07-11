using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Applications;

public interface IPropertyEditingContext
{
	IEnumerable<object> Items { get; }

	IEnumerable<PropertyDescriptor> PropertyDescriptors { get; }
}
