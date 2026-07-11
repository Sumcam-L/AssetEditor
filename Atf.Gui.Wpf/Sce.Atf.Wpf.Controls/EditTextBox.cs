using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls;

public class EditTextBox : TextBox
{
	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		Focus();
		SelectAll();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			BindingOperations.GetBindingExpressionBase(this, TextBox.TextProperty)?.UpdateSource();
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}
}
