using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

public partial class AboutDialog : CommonDialog, IComponentConnector
{
	public AboutDialog()
	{
		InitializeComponent();
	}

	private void hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		if (e.Uri != null && !string.IsNullOrEmpty(e.Uri.OriginalString))
		{
			string absoluteUri = e.Uri.AbsoluteUri;
			Process.Start(new ProcessStartInfo(absoluteUri));
			e.Handled = true;
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		if (!(base.DataContext is AboutDialogViewModel aboutDialogViewModel))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(aboutDialogViewModel.ProductTitle);
		stringBuilder.AppendLine(aboutDialogViewModel.Version);
		foreach (string assembly in aboutDialogViewModel.Assemblies)
		{
			stringBuilder.AppendLine(assembly);
		}
		Clipboard.SetDataObject(stringBuilder.ToString());
	}


}
