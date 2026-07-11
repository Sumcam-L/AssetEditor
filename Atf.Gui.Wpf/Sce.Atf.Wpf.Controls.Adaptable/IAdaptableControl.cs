using System.Collections.Generic;
using System.Windows;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface IAdaptableControl : IAdaptable
{
	void Attach(DependencyObject dependencyObject);

	void Detach(DependencyObject dependencyObject);

	T As<T>() where T : class;

	IEnumerable<T> AsAll<T>() where T : class;
}
