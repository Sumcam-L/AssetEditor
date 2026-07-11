using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Controls;

public partial class OutputView : UserControl, IComponentConnector
{
	public OutputView()
	{
		InitializeComponent();
	}

	private void CtrlCCopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (object selectedItem in lb.SelectedItems)
		{
			if (selectedItem != null)
			{
				stringBuilder.AppendLine(selectedItem.ToString());
			}
		}
		Clipboard.SetText(stringBuilder.ToString());
	}

	private void CtrlCCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = lb.SelectedItems.Count > 0;
	}


}
