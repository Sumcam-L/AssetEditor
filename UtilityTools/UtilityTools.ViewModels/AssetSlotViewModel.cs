using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels;

public class AssetSlotViewModel : Notifier
{
	private DelegateCommand m_browseForAssetCommand;

	private DelegateCommand m_clearAssetCommand;

	private int m_slotID;

	private ObservableCollection<string> m_availableLightRigs;

	private bool m_isClearable = true;

	private bool m_isLightRig;

	private bool m_isPrimarySlot;

	private string m_selectedAsset = "";

	public ObservableCollection<string> AvailableLightRigs
	{
		get
		{
			return m_availableLightRigs;
		}
		set
		{
			if (m_availableLightRigs != value)
			{
				m_availableLightRigs = value;
				OnPropertyChanged("AvailableLightRigs");
			}
		}
	}

	public bool IsClearable
	{
		get
		{
			return m_isClearable;
		}
		private set
		{
			if (m_isClearable != value)
			{
				m_isClearable = value;
				OnPropertyChanged("IsClearable");
			}
		}
	}

	public bool IsLightRig
	{
		get
		{
			return m_isLightRig;
		}
		set
		{
			if (m_isLightRig != value)
			{
				m_isLightRig = value;
				OnPropertyChanged("IsLightRig");
			}
		}
	}

	public bool IsPrimarySlot
	{
		get
		{
			return m_isPrimarySlot;
		}
		set
		{
			if (m_isPrimarySlot != value)
			{
				m_isPrimarySlot = value;
				OnPropertyChanged("IsPrimarySlot");
			}
		}
	}

	public string SelectedAsset
	{
		get
		{
			return m_selectedAsset;
		}
		private set
		{
			if (m_selectedAsset != value)
			{
				m_selectedAsset = value;
				OnPropertyChanged("SelectedAsset");
				OnPropertyChanged("SelectedLightRigIndex");
			}
		}
	}

	public int SelectedLightRigIndex
	{
		get
		{
			return AvailableLightRigs.IndexOf(SelectedAsset);
		}
		set
		{
			if (SelectedLightRigIndex != value)
			{
				SelectedAsset = AvailableLightRigs[value];
				IInstanceEntity instanceByNameAndType = EngineContextWrapper.GetInstanceByNameAndType(InstanceType.IT_LIGHT_RIG, SelectedAsset, Instances);
				if (instanceByNameAndType != null)
				{
					OnSlotChangedEvent(instanceByNameAndType, SlotID);
				}
				OnPropertyChanged("SelectedLightRigIndex");
			}
		}
	}

	public int SlotID
	{
		get
		{
			return m_slotID;
		}
		private set
		{
			if (m_slotID != value)
			{
				m_slotID = value;
				OnPropertyChanged("SlotID");
			}
		}
	}

	protected ICivTechService CivTechService { get; private set; }

	private IEnumerable<string> AllowedClasses { get; set; }

	private IInstanceSet Instances { get; set; }

	private InstanceType SlotAssetType { get; set; }

	public ICommand BrowseForAssetCommand
	{
		get
		{
			if (m_browseForAssetCommand == null)
			{
				m_browseForAssetCommand = new DelegateCommand(ExecuteBrowseForAssetCommand);
			}
			return m_browseForAssetCommand;
		}
	}

	public ICommand ClearAssetCommand
	{
		get
		{
			if (m_clearAssetCommand == null)
			{
				m_clearAssetCommand = new DelegateCommand(ExecuteClearAssetCommand);
			}
			return m_clearAssetCommand;
		}
	}

	public event EventHandler<SlotChangedEventArgs> SlotChangedEvent;

	public event EventHandler<SlotClearedEventArgs> SlotClearedEvent;

	public AssetSlotViewModel(ICivTechService civTechSvc, IEnumerable<string> allowedClasses, int slotID, InstanceType instanceType, IInstanceSet instances, bool isClearable)
		: this(civTechSvc, allowedClasses, slotID, "", instanceType, instances, isClearable)
	{
	}

	public AssetSlotViewModel(ICivTechService civTechSvc, IEnumerable<string> allowedClasses, int slotID, string selectedAsset, InstanceType instanceType, IInstanceSet instances, bool isClearable)
	{
		CivTechService = civTechSvc;
		if (allowedClasses == null)
		{
			throw new ArgumentNullException("Allowed classes cannot be null");
		}
		AvailableLightRigs = new ObservableCollection<string>();
		AllowedClasses = allowedClasses;
		SlotID = slotID;
		SelectedAsset = ((selectedAsset == null) ? "" : selectedAsset);
		SlotAssetType = instanceType;
		IsLightRig = instanceType.Equals(InstanceType.IT_LIGHT_RIG);
		IsClearable = isClearable;
		Instances = instances;
		if (IsLightRig)
		{
			UpdateAvailableLightRigs(selectedAsset);
		}
	}

	public void UpdateAvailableLightRigs(string selectedAsset)
	{
		AvailableLightRigs.Clear();
		ICivTechService civTechService = CivTechRegistry.CivTechService;
		IQueryService queryService = CivTechRegistry.EntityQueryService.FindFilesByName(new string[1] { civTechService.AnyProject }, string.Empty, AllowedClasses, new InstanceType[1] { InstanceType.IT_LIGHT_RIG });
		queryService.InstanceItems[InstanceType.IT_LIGHT_RIG].ForEach(delegate(string name)
		{
			AvailableLightRigs.Add(name);
		});
		SelectedAsset = selectedAsset;
		OnPropertyChanged("AvailableLightRigs");
		OnPropertyChanged("SelectedLightRigIndex");
	}

	protected virtual void OnSlotChangedEvent(IInstanceEntity entity, int slotID)
	{
		this.SlotChangedEvent?.Invoke(this, new SlotChangedEventArgs(entity, slotID));
	}

	protected virtual void OnSlotClearedEvent(int slotID)
	{
		this.SlotClearedEvent?.Invoke(this, new SlotClearedEventArgs(slotID));
	}

	private void ExecuteBrowseForAssetCommand(object context)
	{
		IInstanceEntity instanceEntity = DialogHelper.PickAndLoadAssetObject(Application.Current.MainWindow, CivTechService, new InstanceType[1] { SlotAssetType }, AllowedClasses, Instances);
		if (instanceEntity != null)
		{
			SelectedAsset = instanceEntity.Name;
			OnSlotChangedEvent(instanceEntity, SlotID);
		}
	}

	private void ExecuteClearAssetCommand(object context)
	{
		SelectedAsset = string.Empty;
		OnSlotClearedEvent(SlotID);
	}
}
