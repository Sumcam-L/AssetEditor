using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyEditedEventArgs : EventArgs
{
	public readonly object Owner;

	public readonly PropertyDescriptor Descriptor;

	public readonly object OldValue;

	public readonly object NewValue;

	public PropertyEditedEventArgs(object owner, PropertyDescriptor descriptor, object oldValue, object newValue)
	{
		Owner = owner;
		Descriptor = descriptor;
		OldValue = oldValue;
		NewValue = newValue;
	}
}
