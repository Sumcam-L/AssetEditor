using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class CompositeOpenedEventArgs : EventArgs
{
	public readonly object Part;

	public readonly PropertyDescriptor[] Descriptors;

	public CompositeOpenedEventArgs(object part, PropertyDescriptor[] descriptors)
	{
		Part = part;
		Descriptors = descriptors;
	}
}
