using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

[Export(typeof(MainWindow))]
[Export(typeof(Window))]
[Export(typeof(IMainWindowContentSite))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public partial class MainWindow : Window, IMainWindowContentSite, IInitializable, INotifyPropertyChanged, IComponentConnector
{
	private static readonly string s_toolBarsPropertyName = TypeUtil.GetProperty((ToolBarViewModel x) => x.ToolBars).Name;

	private ToolBarViewModel m_toolBarViewModel;

	private MainMenuViewModel m_mainMenuViewModel;

	private IStatusItem[] m_statusItems;

	private Control m_mainContent;

	[Import("ViewModel")]
	public ToolBarViewModel ToolBarViewModel
	{
		get
		{
			return m_toolBarViewModel;
		}
		set
		{
			m_toolBarViewModel = value;
			BuildToolBars();
			m_toolBarViewModel.PropertyChanged += ToolBarViewModel_PropertyChanged;
		}
	}

	[Import("ViewModel")]
	public MainMenuViewModel MainMenuViewModel
	{
		get
		{
			return m_mainMenuViewModel;
		}
		set
		{
			m_mainMenuViewModel = value;
			OnPropertyChanged(new PropertyChangedEventArgs("MainMenuViewModel"));
		}
	}

	[ImportMany(AllowRecomposition = true)]
	public IStatusItem[] StatusItems
	{
		get
		{
			return m_statusItems;
		}
		set
		{
			m_statusItems = value;
			OnPropertyChanged(new PropertyChangedEventArgs("StatusItems"));
		}
	}

	public Control MainContent
	{
		get
		{
			return m_mainContent;
		}
		set
		{
			value.SetValue(DockPanel.DockProperty, Dock.Top);
			dockPanel.Children.Add(value);
			m_mainContent = value;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public MainWindow()
	{
		InitializeComponent();
	}

	public void Initialize()
	{
	}

	protected void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	private void BuildToolBars()
	{
		toolBarTray.ToolBars.Clear();
		IToolBar[] toolBars = ToolBarViewModel.ToolBars;
		foreach (IToolBar toolBar in toolBars)
		{
			IEnumerable<IToolBarItem> toolBarItems = ToolBarViewModel.GetToolBarItems(toolBar);
			if (toolBarItems.Any())
			{
				ToolBar toolBar2 = new ToolBar();
				toolBar2.SetResourceReference(FrameworkElement.StyleProperty, Sce.Atf.Wpf.Resources.ToolBarStyleKey);
				toolBar2.DataContext = toolBar;
				toolBar2.ItemsSource = toolBarItems;
				toolBar2.ItemTemplateSelector = ToolBarItemTemplateSelector.Instance;
				toolBarTray.ToolBars.Add(toolBar2);
			}
		}
	}

	private void ToolBarViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == s_toolBarsPropertyName)
		{
			BuildToolBars();
		}
	}


}
