using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels;

public class PreviewerAndAssetSelectorViewModel : Notifier
{
	private ObservableCollection<string> m_availablePreviewModules;

	private string m_selectedAssetName;

	private DelegateCommand m_selectAssetCommand;

	private string m_selectedPreviewModule;

	public ObservableCollection<string> AvailablePreviewModules
	{
		get
		{
			return m_availablePreviewModules;
		}
		private set
		{
			if (m_availablePreviewModules != value)
			{
				m_availablePreviewModules = value;
				OnPropertyChanged("AvailablePreviewModules");
			}
		}
	}

	public bool CanPreviewLightClass => AvailablePreviewModules.Count > 0;

	public bool HasSelectedPreviewModule => !string.IsNullOrEmpty(SelectedPreviewModule);

	public string SelectedAssetName
	{
		get
		{
			return m_selectedAssetName;
		}
		private set
		{
			if (m_selectedAssetName != value)
			{
				m_selectedAssetName = value;
				OnPropertyChanged("SelectedAssetName");
			}
		}
	}

	public string SelectedPreviewModule
	{
		get
		{
			return m_selectedPreviewModule;
		}
		set
		{
			if (m_selectedPreviewModule != value && value != null)
			{
				m_selectedPreviewModule = value;
				OnPropertyChanged("SelectedPreviewModule");
				OnPropertyChanged("HasSelectedPreviewModule");
				OnSelectedPreviewModuleChanged(value);
			}
		}
	}

	public int SelectedPreviewModuleIndex
	{
		get
		{
			return AvailablePreviewModules.IndexOf(SelectedPreviewModule);
		}
		set
		{
			if (SelectedPreviewModuleIndex != value)
			{
				SelectedPreviewModule = AvailablePreviewModules[value];
				OnPropertyChanged("SelectedPreviewModuleIndex");
			}
		}
	}

	protected ICivTechService CivTechService { get; private set; }

	private IInstanceSet Instances { get; set; }

	private IPreviewerHelper PreviewerHelper { get; set; }

	public ICommand SelectAssetCommand
	{
		get
		{
			if (m_selectAssetCommand == null)
			{
				m_selectAssetCommand = new DelegateCommand(ExecuteSelectAssetCommand);
			}
			return m_selectAssetCommand;
		}
	}

	public event EventHandler<SelectedPreviewModuleChangedEventArgs> SelectedPreviewModuleChangedEvent;

	public event EventHandler<SelectedAssetChangedEventArgs> SelectedAssetChangedEvent;

	public PreviewerAndAssetSelectorViewModel(ICivTechService civTechSvc, IInstanceSet instances, IPreviewerHelper previewHelper)
	{
		if (previewHelper == null)
		{
			throw new ArgumentNullException("The argument previewHelper cannot be set to null.");
		}
		if (instances == null)
		{
			throw new ArgumentNullException("The argument instances cannot be set to null.");
		}
		CivTechService = civTechSvc;
		Instances = instances;
		AvailablePreviewModules = new ObservableCollection<string>();
		SelectedPreviewModule = string.Empty;
		PreviewerHelper = previewHelper;
	}

	public void SetSelectedAsset(string assetName)
	{
		if (!string.IsNullOrEmpty(SelectedPreviewModule) && Instances.LoadEntityByName(assetName, InstanceType.IT_ASSET) is IAssetInstance assetInstance)
		{
			IEnumerable<string> assetClassesThatSupportPreviewer = PreviewerHelper.GetAssetClassesThatSupportPreviewer(SelectedPreviewModule);
			if (assetClassesThatSupportPreviewer.Contains(assetInstance.ClassName))
			{
				SelectedAssetName = assetInstance.Name;
				OnSelectedAssetChanged(assetInstance);
			}
		}
	}

	public void UpdateAvailablePreviewerModules(IEnumerable<string> previewModuleNames, string selectedPreviewModule)
	{
		AvailablePreviewModules = new ObservableCollection<string>(previewModuleNames);
		SelectedPreviewModule = selectedPreviewModule;
		if (string.IsNullOrEmpty(SelectedPreviewModule) && AvailablePreviewModules.Count > 0)
		{
			SelectedPreviewModule = AvailablePreviewModules[0];
		}
		OnPropertyChanged("AvailablePreviewModules");
		OnPropertyChanged("CanPreviewLightClass");
		OnPropertyChanged("SelectedPreviewModuleIndex");
		OnSelectedPreviewModuleChanged(m_selectedPreviewModule);
	}

	protected virtual void OnSelectedAssetChanged(IAssetInstance asset)
	{
		this.SelectedAssetChangedEvent?.Invoke(this, new SelectedAssetChangedEventArgs(asset));
	}

	protected virtual void OnSelectedPreviewModuleChanged(string previewModuleName)
	{
		this.SelectedPreviewModuleChangedEvent?.Invoke(this, new SelectedPreviewModuleChangedEventArgs(previewModuleName));
	}

	private void ExecuteSelectAssetCommand(object context)
	{
		if (!string.IsNullOrEmpty(SelectedPreviewModule))
		{
			IEnumerable<string> assetClassesThatSupportPreviewer = PreviewerHelper.GetAssetClassesThatSupportPreviewer(SelectedPreviewModule);
			if (DialogHelper.PickAndLoadAssetObject(Application.Current.MainWindow, CivTechService, InstanceType.IT_ASSET, assetClassesThatSupportPreviewer, Instances) is IAssetInstance assetInstance)
			{
				SelectedAssetName = assetInstance.Name;
				OnSelectedAssetChanged(assetInstance);
			}
		}
	}
}
