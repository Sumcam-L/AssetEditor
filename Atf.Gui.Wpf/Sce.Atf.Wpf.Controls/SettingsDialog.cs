using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

public partial class SettingsDialog : CommonDialog, IComponentConnector
{
	public SettingsDialog(DialogViewModelBase viewModel)
	{
		InitializeComponent();
		base.DataContext = viewModel;
	}


}
