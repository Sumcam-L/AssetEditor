using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Models;

internal class SettingsDialogViewModel : DialogViewModelBase
{
	private IEnumerable m_item;

	private IEnumerable<PropertyDescriptor> m_properties;

	private readonly TreeViewWithSelection m_treeViewAdapter;

	private readonly SettingsService m_settingsService;

	private readonly object m_originalState;

	public ICommand SetDefaultsCommand { get; private set; }

	public object ImageProvider { get; private set; }

	public TreeViewModel TreeViewModel { get; private set; }

	public IEnumerable<PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			return m_properties;
		}
		set
		{
			m_properties = value;
			RaisePropertyChanged("PropertyDescriptors");
		}
	}

	public IEnumerable Items
	{
		get
		{
			yield return m_item;
		}
		set
		{
			m_item = value;
			RaisePropertyChanged("Items");
		}
	}

	public SettingsDialogViewModel(SettingsService settingsService, string pathName)
	{
		base.Title = "Preferences".Localize();
		m_settingsService = settingsService;
		m_originalState = m_settingsService.UserState;
		m_treeViewAdapter = new TreeViewWithSelection(settingsService.UserSettingsInternal);
		m_treeViewAdapter.SelectionChanged += TreeViewAdapterSelectionChanged;
		TreeViewModel = new TreeViewModel
		{
			MultiSelectEnabled = false,
			ShowRoot = false,
			TreeView = m_treeViewAdapter
		};
		TreeViewModel.ExpandAll();
		((pathName != null) ? TreeViewModel.Show(m_settingsService.GetSettingsPathInternal(pathName), select: true) : TreeViewModel.ExpandToFirstLeaf()).IsSelected = true;
		SetDefaultsCommand = new DelegateCommand(SetDefaults, CanSetDefaults, isAutomaticRequeryDisabled: false);
	}

	protected bool CanSetDefaults()
	{
		return true;
	}

	protected override void OnCloseDialog(CloseDialogEventArgs args)
	{
		if (args.DialogResult != true)
		{
			m_settingsService.UserState = m_originalState;
		}
		RaiseCloseDialog(args);
	}

	private void SetDefaults()
	{
		MessageBoxResult messageBoxResult = WpfMessageBox.Show("Reset all preferences to their default values?".Localize(), "Reset All Preferences".Localize(), MessageBoxButton.OKCancel);
		if (messageBoxResult == MessageBoxResult.OK)
		{
			m_settingsService.SetDefaults();
		}
	}

	private void TreeViewAdapterSelectionChanged(object sender, EventArgs e)
	{
		Tree<object> lastSelected = m_treeViewAdapter.GetLastSelected<Tree<object>>();
		if (lastSelected != null)
		{
			Items = new Tree<object>[1] { lastSelected };
			List<PropertyDescriptor> propertiesInternal = m_settingsService.GetPropertiesInternal(lastSelected);
			if (propertiesInternal != null)
			{
				PropertyDescriptors = propertiesInternal.ToArray();
			}
		}
	}
}
