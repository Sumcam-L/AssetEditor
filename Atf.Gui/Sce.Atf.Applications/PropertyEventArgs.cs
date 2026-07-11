using System;
using System.ComponentModel;

namespace Sce.Atf.Applications;

public class PropertyEventArgs : EventArgs
{
	public readonly object Owner;

	public readonly PropertyDescriptor Descriptor;

	public readonly object OldValue;

	public readonly object NewValue;

	public PropertyEventArgs(object owner, PropertyDescriptor descriptor, object oldValue, object newValue)
	{
		Owner = owner;
		Descriptor = descriptor;
		OldValue = oldValue;
		NewValue = newValue;
	}
}
