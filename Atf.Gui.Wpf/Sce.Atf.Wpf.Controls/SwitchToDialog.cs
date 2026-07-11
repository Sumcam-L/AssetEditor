using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Controls;

internal partial class SwitchToDialog : CommonDialog, IComponentConnector
{
	private static SwitchToDialog s_currentlyActiveDialog;

	private bool m_cancel;

	private readonly ObservableCollection<IControlInfo> m_controlInfos;

	private readonly ICollectionView m_controlInfoView;

	private readonly IControlHostService m_controlHostService;

	public ObservableCollection<IControlInfo> ControlInfos => m_controlInfos;

	public static bool IsInUse => s_currentlyActiveDialog != null;

	protected override bool IsOverridingWindowsChrome => false;

	public SwitchToDialog(IControlHostService controlHostService)
	{
		InitializeComponent();
		base.DataContext = this;
		m_controlHostService = controlHostService;
		m_controlInfos = new ObservableCollection<IControlInfo>();
		foreach (IControlInfo content in m_controlHostService.Contents)
		{
			m_controlInfos.Add(content);
		}
		base.Loaded += OnLoaded;
		m_controlInfoView = CollectionViewSource.GetDefaultView(m_controlInfos);
		m_controlInfoView.MoveCurrentToFirst();
		MoveSelectionByOne(forwards: true);
	}

	public static void FocusCurrentInstance()
	{
		if (s_currentlyActiveDialog != null)
		{
			s_currentlyActiveDialog.Focus();
		}
	}

	protected override void OnClosed(EventArgs e)
	{
		if (!m_cancel)
		{
			IControlInfo controlInfo = (IControlInfo)m_controlInfoView.CurrentItem;
			if (controlInfo != null)
			{
				m_controlHostService.Show(controlInfo.Content);
			}
		}
		s_currentlyActiveDialog = null;
		base.OnClosed(e);
	}

	protected override void OnInitialized(EventArgs e)
	{
		s_currentlyActiveDialog = this;
		base.OnInitialized(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Tab)
		{
			if (e.IsDown)
			{
				bool flag = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);
				MoveSelectionByOne(!flag);
			}
		}
		else if (e.Key == Key.Up)
		{
			if (e.IsDown)
			{
				MoveSelectionByOne(forwards: false);
			}
		}
		else if (e.Key == Key.Down)
		{
			if (e.IsDown)
			{
				MoveSelectionByOne(forwards: true);
			}
		}
		else if (e.Key != Key.LeftShift && e.Key != Key.RightShift)
		{
			m_cancel = true;
			Close();
		}
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
		{
			Close();
		}
		else
		{
			base.OnKeyUp(e);
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
	{
		Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)delegate
		{
			try
			{
				if (PresentationSource.FromVisual(this) != null)
				{
					MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
				}
			}
			catch (Win32Exception)
			{
			}
		});
	}

	private void MoveSelectionByOne(bool forwards)
	{
		if (m_controlInfoView == null)
		{
			return;
		}
		if (!forwards)
		{
			m_controlInfoView.MoveCurrentToPrevious();
			if (m_controlInfoView.CurrentItem == null)
			{
				m_controlInfoView.MoveCurrentToLast();
			}
		}
		else
		{
			m_controlInfoView.MoveCurrentToNext();
			if (m_controlInfoView.CurrentItem == null)
			{
				m_controlInfoView.MoveCurrentToFirst();
			}
		}
		ControlList.ScrollIntoView(m_controlInfoView.CurrentItem);
	}


}
