using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

public partial class WindowLayoutNewDialog : CommonDialog, IComponentConnector
{
	public WindowLayoutNewDialog(DialogViewModelBase vm)
	{
		InitializeComponent();
		base.DataContext = vm;
	}


}
