using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyErrorEventArgs : CancelEventArgs
{
	public object Owner;

	public PropertyDescriptor Descriptor;

	public readonly Exception Exception;

	public PropertyErrorEventArgs(object owner, PropertyDescriptor descriptor, Exception exception)
	{
		Owner = owner;
		Descriptor = descriptor;
		Exception = exception;
	}
}
