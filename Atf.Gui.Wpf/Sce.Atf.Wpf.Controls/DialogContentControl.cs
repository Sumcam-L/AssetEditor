using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

public abstract class DialogContentControl : UserControl, IDialogContent
{
	public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(object), typeof(DialogContentControl), new PropertyMetadata((object)null));

	public static readonly DependencyProperty ShowInTaskbarProperty = DependencyProperty.Register("ShowInTaskbar", typeof(bool), typeof(DialogContentControl), new PropertyMetadata(true));

	public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register("SizeToContent", typeof(SizeToContent), typeof(DialogContentControl), new PropertyMetadata(SizeToContent.Manual));

	public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register("ResizeMode", typeof(ResizeMode), typeof(DialogContentControl), new PropertyMetadata(ResizeMode.CanResize));

	public static readonly DependencyProperty DialogWidthProperty = DependencyProperty.Register("DialogWidth", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

	public static readonly DependencyProperty DialogMinWidthProperty = DependencyProperty.Register("DialogMinWidth", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

	public static readonly DependencyProperty DialogHeightProperty = DependencyProperty.Register("DialogHeight", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

	public static readonly DependencyProperty DialogMinHeightProperty = DependencyProperty.Register("DialogMinHeight", typeof(double), typeof(DialogContentControl), new PropertyMetadata(double.NaN));

	private IDialogContentHost m_host;

	private IDialogViewModel m_viewModel;

	private bool m_closing;

	public WindowStartupLocation WindowStartupLocation { get; set; }

	public object Title
	{
		get
		{
			return GetValue(TitleProperty);
		}
		set
		{
			SetValue(TitleProperty, value);
		}
	}

	public bool ShowInTaskbar
	{
		get
		{
			return (bool)GetValue(ShowInTaskbarProperty);
		}
		set
		{
			SetValue(ShowInTaskbarProperty, value);
		}
	}

	public SizeToContent SizeToContent
	{
		get
		{
			return (SizeToContent)GetValue(SizeToContentProperty);
		}
		set
		{
			SetValue(SizeToContentProperty, value);
		}
	}

	public ResizeMode ResizeMode
	{
		get
		{
			return (ResizeMode)GetValue(ResizeModeProperty);
		}
		set
		{
			SetValue(ResizeModeProperty, value);
		}
	}

	public double DialogWidth
	{
		get
		{
			return (double)GetValue(DialogWidthProperty);
		}
		set
		{
			SetValue(DialogWidthProperty, value);
		}
	}

	public double DialogMinWidth
	{
		get
		{
			return (double)GetValue(DialogMinWidthProperty);
		}
		set
		{
			SetValue(DialogMinWidthProperty, value);
		}
	}

	public double DialogHeight
	{
		get
		{
			return (double)GetValue(DialogHeightProperty);
		}
		set
		{
			SetValue(DialogHeightProperty, value);
		}
	}

	public double DialogMinHeight
	{
		get
		{
			return (double)GetValue(DialogMinHeightProperty);
		}
		set
		{
			SetValue(DialogMinHeightProperty, value);
		}
	}

	public IDialogContentHost Host
	{
		get
		{
			return m_host;
		}
		set
		{
			if (m_host != null)
			{
				m_host.DialogClosing -= OnDialogClosing;
			}
			m_host = value;
			if (m_host != null)
			{
				m_host.DialogClosing += OnDialogClosing;
				if (m_host is Window window)
				{
					window.SetBinding(Window.TitleProperty, new Binding("Title")
					{
						Source = this
					});
					window.WindowStartupLocation = WindowStartupLocation;
					window.SetBinding(Window.ShowInTaskbarProperty, new Binding("ShowInTaskbar")
					{
						Source = this
					});
					window.SetBinding(Window.SizeToContentProperty, new Binding("SizeToContent")
					{
						Source = this
					});
					window.SetBinding(Window.ResizeModeProperty, new Binding("ResizeMode")
					{
						Source = this
					});
					window.SetBinding(FrameworkElement.WidthProperty, new Binding("DialogWidth")
					{
						Source = this
					});
					window.SetBinding(FrameworkElement.HeightProperty, new Binding("DialogHeight")
					{
						Source = this
					});
					window.SetBinding(FrameworkElement.MinWidthProperty, new Binding("DialogMinWidth")
					{
						Source = this
					});
					window.SetBinding(FrameworkElement.MinHeightProperty, new Binding("DialogMinHeight")
					{
						Source = this
					});
				}
			}
		}
	}

	protected DialogContentControl()
	{
		WindowStartupLocation = WindowStartupLocation.CenterOwner;
		base.DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (m_viewModel != null)
		{
			m_viewModel.CloseDialog -= ViewModel_CloseDialog;
		}
		m_viewModel = base.DataContext as IDialogViewModel;
		if (m_viewModel != null)
		{
			m_viewModel.CloseDialog += ViewModel_CloseDialog;
		}
	}

	protected virtual void OnDialogClosing(object sender, HostClosingEventArgs e)
	{
		IDialogViewModel viewModel = m_viewModel;
		if (!m_closing && viewModel != null && viewModel.CancelCommand != null)
		{
			if (!viewModel.CancelCommand.CanExecute(null))
			{
				e.Cancel = true;
			}
			else
			{
				try
				{
					m_closing = true;
					viewModel.CancelCommand.Execute(null);
				}
				finally
				{
					m_closing = false;
				}
			}
		}
		if (!e.Cancel)
		{
			base.DataContext = null;
		}
	}

	private void ViewModel_CloseDialog(object sender, CloseDialogEventArgs e)
	{
		if (!m_closing)
		{
			try
			{
				m_closing = true;
				m_host.RequestClose(e.DialogResult);
			}
			finally
			{
				m_closing = false;
			}
		}
	}
}
