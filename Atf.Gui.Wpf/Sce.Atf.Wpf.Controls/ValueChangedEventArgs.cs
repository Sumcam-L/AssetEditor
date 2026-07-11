using System;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Controls;

public class ValueChangedEventArgs : EventArgs
{
	private PropertyDescriptor m_pd;

	public PropertyDescriptor PropertyDescriptor => m_pd;

	public ValueChangedEventArgs(PropertyDescriptor pd)
	{
		m_pd = pd;
	}
}
