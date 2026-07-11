using System;
using System.ComponentModel;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public static class DefaultPropertyGrouping
{
	public static GroupDescription None => null;

	public static GroupDescription ByCategory => new PropertyGroupDescription("Category", null, StringComparison.CurrentCultureIgnoreCase);
}
