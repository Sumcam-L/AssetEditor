using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

internal partial class TargetDialog : CommonDialog, IComponentConnector
{
	public TargetDialog()
	{
		InitializeComponent();
		base.DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext is TargetDialogViewModel targetDialogViewModel)
		{
			targetDialogViewModel.ShowFindTargetsDialog += delegate(object s, ShowDialogEventArgs args)
			{
				FindTargetsDialog findTargetsDialog = Activator.CreateInstance<FindTargetsDialog>();
				findTargetsDialog.DataContext = args.ViewModel;
				findTargetsDialog.Owner = Application.Current.MainWindow;
				findTargetsDialog.ShowDialog();
			};
		}
	}


}
