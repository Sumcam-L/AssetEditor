using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

internal partial class FindTargetsDialog : CommonDialog, IComponentConnector
{
	public FindTargetsDialog()
	{
		InitializeComponent();
		base.Closing += delegate
		{
			CancelScan();
		};
		base.Loaded += delegate
		{
			StartScan();
		};
	}

	private void CancelScan()
	{
		if (base.DataContext is FindTargetsViewModel { IsScanning: not false } findTargetsViewModel)
		{
			findTargetsViewModel.ToggleScanCommand.Execute(null);
		}
	}

	private void StartScan()
	{
		if (base.DataContext is FindTargetsViewModel { IsScanning: false } findTargetsViewModel)
		{
			findTargetsViewModel.ToggleScanCommand.Execute(null);
		}
	}


}
