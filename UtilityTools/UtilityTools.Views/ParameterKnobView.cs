using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using UtilityTools.ViewModels.ParameterViewModels;

namespace UtilityTools.Views;

public partial class ParameterKnobView : System.Windows.Controls.UserControl, IComponentConnector, IStyleConnector
{
	public ParameterKnobView()
	{
		InitializeComponent();
	}

	private void RGB_Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
	{
		ColorDialog colorDialog = new ColorDialog();
		colorDialog.SolidColorOnly = true;
		if (colorDialog.ShowDialog() == DialogResult.OK)
		{
			RGBParameterViewModel rGBParameterViewModel = parameter.Content as RGBParameterViewModel;
			rGBParameterViewModel.RValue = colorDialog.Color.R.ToString();
			rGBParameterViewModel.GValue = colorDialog.Color.G.ToString();
			rGBParameterViewModel.BValue = colorDialog.Color.B.ToString();
		}
	}


}
