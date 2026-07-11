using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Controls;

public partial class MessageBoxDialog : CommonDialog, IComponentConnector
{
	public string YesButtonText
	{
		get
		{
			return (YesButton.Content as TextBlock).Text;
		}
		set
		{
			(YesButton.Content as TextBlock).Text = value;
		}
	}

	public string NoButtonText
	{
		get
		{
			return (NoButton.Content as TextBlock).Text;
		}
		set
		{
			(NoButton.Content as TextBlock).Text = value;
		}
	}

	public string OkButtonText
	{
		get
		{
			return (OkButton.Content as TextBlock).Text;
		}
		set
		{
			(OkButton.Content as TextBlock).Text = value;
		}
	}

	public string CancelButtonText
	{
		get
		{
			return (CancelButton.Content as TextBlock).Text;
		}
		set
		{
			(CancelButton.Content as TextBlock).Text = value;
		}
	}

	public MessageBoxResult MessageBoxResult { get; private set; }

	public MessageBoxDialog()
	{
		InitializeComponent();
	}

	public MessageBoxDialog(string title, string message, MessageBoxButton buttons, MessageBoxImage image)
		: this()
	{
		base.Loaded += MessageBoxDialog_Loaded;
		MessageBoxResult = MessageBoxResult.None;
		OkButton.Visibility = ((buttons != MessageBoxButton.OKCancel && buttons != MessageBoxButton.OK) ? Visibility.Collapsed : Visibility.Visible);
		YesButton.Visibility = (NoButton.Visibility = ((buttons != MessageBoxButton.YesNo && buttons != MessageBoxButton.YesNoCancel) ? Visibility.Collapsed : Visibility.Visible));
		CancelButton.Visibility = ((buttons != MessageBoxButton.OKCancel && buttons != MessageBoxButton.YesNoCancel) ? Visibility.Collapsed : Visibility.Visible);
		if (OkButton.Visibility == Visibility.Visible)
		{
			OkButton.IsDefault = true;
		}
		else
		{
			YesButton.IsDefault = true;
		}
		if (message != null)
		{
			MessageText.Text = message;
		}
		if (title != null)
		{
			base.Title = title;
		}
		object obj = null;
		switch (image)
		{
		case MessageBoxImage.Hand:
			obj = Sce.Atf.Wpf.Resources.DialogErrorImageKey;
			break;
		case MessageBoxImage.Question:
			obj = Sce.Atf.Wpf.Resources.DialogQuestionImageKey;
			break;
		case MessageBoxImage.Exclamation:
			obj = Sce.Atf.Wpf.Resources.DialogWarningImageKey;
			break;
		case MessageBoxImage.Asterisk:
			obj = Sce.Atf.Wpf.Resources.DialogInformationImageKey;
			break;
		}
		if (obj != null)
		{
			Sce.Atf.Wpf.Controls.Icon.SetSourceKey(Image, obj);
		}
		else
		{
			Image.Visibility = Visibility.Collapsed;
		}
	}

	private void OkButton_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult = MessageBoxResult.OK;
		base.DialogResult = true;
		Close();
	}

	private void MessageBoxDialog_Loaded(object sender, RoutedEventArgs e)
	{
		if (OkButton.Visibility == Visibility.Visible)
		{
			OkButton.Focus();
		}
		else
		{
			YesButton.Focus();
		}
	}

	private void YesButton_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult = MessageBoxResult.Yes;
		base.DialogResult = true;
		Close();
	}

	private void NoButton_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult = MessageBoxResult.No;
		base.DialogResult = false;
		Close();
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		MessageBoxResult = MessageBoxResult.Cancel;
		base.DialogResult = false;
		Close();
	}


}
