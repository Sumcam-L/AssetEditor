using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using UtilityTools.ViewModels;

namespace UtilityTools.Views;

public partial class InstanceEntityView : UserControl, IComponentConnector
{
	public IInstanceEntityViewModel ViewModel => (IInstanceEntityViewModel)base.DataContext;

	public InstanceEntityView(IInstanceEntityViewModel viewModel)
	{
		InitializeComponent();
		base.DataContext = viewModel;
	}


}
