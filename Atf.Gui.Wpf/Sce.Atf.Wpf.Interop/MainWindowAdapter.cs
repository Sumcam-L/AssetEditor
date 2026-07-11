using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Interop;

[Export(typeof(IMainWindow))]
[Export(typeof(MainWindowAdapter))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MainWindowAdapter : IMainWindow, IInitializable
{
	private class Shim : System.Windows.Forms.IWin32Window
	{
		private WindowInteropHelper interopHelper;

		public IntPtr Handle => interopHelper.Handle;

		public Shim(Window owner)
		{
			interopHelper = new WindowInteropHelper(owner);
		}
	}

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	private Rect m_mainFormBounds;

	public Window MainWindow { get; private set; }

	public string Text
	{
		get
		{
			return MainWindow.Title;
		}
		set
		{
			MainWindow.Dispatcher.InvokeIfRequired(() => MainWindow.Title = value);
		}
	}

	public System.Windows.Forms.IWin32Window DialogOwner => new Shim(MainWindow);

	public Rect MainFormBounds
	{
		get
		{
			return m_mainFormBounds;
		}
		set
		{
			MainWindow.Width = Math.Max(value.Width, SystemParameters.MinimumWindowWidth);
			MainWindow.Height = Math.Max(value.Height, SystemParameters.MinimumWindowHeight);
			MainWindow.Left = Math.Max(SystemParameters.VirtualScreenLeft, Math.Min(value.X, SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - MainWindow.Width));
			MainWindow.Top = Math.Max(SystemParameters.VirtualScreenTop, Math.Min(value.Y, SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - MainWindow.Height));
		}
	}

	public WindowState MainFormWindowState
	{
		get
		{
			WindowState windowState = MainWindow.WindowState;
			if (windowState == WindowState.Minimized)
			{
				windowState = WindowState.Normal;
			}
			return windowState;
		}
		set
		{
			MainWindow.WindowState = value;
		}
	}

	public event EventHandler Loading;

	public event EventHandler Loaded;

	public event CancelEventHandler Closing;

	public event EventHandler Closed;

	[ImportingConstructor]
	public MainWindowAdapter(Window mainWindow)
	{
		MainWindow = mainWindow;
		MainWindow.Title = GetProductTitle();
		MainWindow.Loaded += delegate
		{
			this.Loaded.Raise(this, EventArgs.Empty);
		};
		MainWindow.Closing += mainWindow_Closing;
		MainWindow.Closed += delegate
		{
			this.Closed.Raise(this, EventArgs.Empty);
		};
		MainWindow.SizeChanged += delegate(object s, SizeChangedEventArgs e)
		{
			StoreBounds(s as Window);
		};
		MainWindow.LocationChanged += delegate(object s, EventArgs e)
		{
			StoreBounds(s as Window);
		};
	}

	public void Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => MainFormBounds, "MainFormBounds", null, null), new BoundPropertyDescriptor(this, () => MainFormWindowState, "MainFormWindowState", null, null));
		}
	}

	public void Close()
	{
		MainWindow.Close();
	}

	public void ShowMainWindow()
	{
		this.Loading.Raise(this, EventArgs.Empty);
		MainWindow.Show();
	}

	private string GetProductTitle()
	{
		object[] customAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((AssemblyProductAttribute)customAttributes[0]).Product;
		}
		customAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((AssemblyTitleAttribute)customAttributes[0]).Title;
		}
		return null;
	}

	private void StoreBounds(Window wnd)
	{
		if (wnd.WindowState == WindowState.Normal)
		{
			m_mainFormBounds = new Rect(wnd.Left, wnd.Top, wnd.Width, wnd.Height);
		}
		else
		{
			m_mainFormBounds = new Rect(wnd.RestoreBounds.Left, wnd.RestoreBounds.Top, wnd.RestoreBounds.Width, wnd.RestoreBounds.Height);
		}
	}

	private void mainWindow_Closing(object sender, CancelEventArgs e)
	{
		CancelEventHandler cancelEventHandler = this.Closing;
		if (cancelEventHandler != null)
		{
			CancelEventArgs e2 = new CancelEventArgs();
			cancelEventHandler(this, e2);
			e.Cancel = e2.Cancel;
		}
	}
}
