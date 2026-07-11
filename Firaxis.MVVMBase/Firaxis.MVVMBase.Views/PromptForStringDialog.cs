using System.Windows;
using System.Windows.Markup;

namespace Firaxis.MVVMBase.Views;

public partial class PromptForStringDialog : Window, IComponentConnector
{
	public string userString { get; set; }

	public string MessagePrompt
	{
		get
		{
			return TextMessage.Text;
		}
		set
		{
			TextMessage.Text = value;
		}
	}

	public PromptForStringDialog()
	{
		InitializeComponent();
	}

	private void OnTextChanged(object sender, RoutedEventArgs e)
	{
		userString = TxtBox.Text;
	}

	private void OnOKButtonClick(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		Close();
	}

	private void OnCancelButtonClick(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}
}
