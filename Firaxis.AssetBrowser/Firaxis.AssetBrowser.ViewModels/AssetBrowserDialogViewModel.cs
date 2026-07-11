using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

public class AssetBrowserDialogViewModel : BaseViewModel
{
	protected AssetBrowserViewModel _assetBrowser;

	protected ICivTechService _civTechService;

	protected bool? _dialogResult;

	protected bool _requestedClose;

	protected RelayCommand _acceptCommand;

	protected RelayCommand _cancelCommand;

	public AssetBrowserViewModel AssetBrowser
	{
		get
		{
			return _assetBrowser;
		}
		protected set
		{
			if (_assetBrowser != value)
			{
				_assetBrowser = value;
				OnPropertyChanged("AssetBrowser");
			}
		}
	}

	public ICivTechService CivTechService
	{
		get
		{
			return _civTechService;
		}
		protected set
		{
			if (_civTechService != value)
			{
				_civTechService = value;
				OnPropertyChanged("CivTechService");
			}
		}
	}

	public bool? DialogResult
	{
		get
		{
			return _dialogResult;
		}
		protected set
		{
			if (_dialogResult != value)
			{
				_dialogResult = value;
				OnPropertyChanged("DialogResult");
			}
		}
	}

	public bool RequestedClose
	{
		get
		{
			return _requestedClose;
		}
		set
		{
			if (_requestedClose != value)
			{
				_requestedClose = value;
				OnPropertyChanged("RequestedClose");
			}
		}
	}

	public bool HasSingleSelection => AssetBrowser.SelectedEntity != null;

	public bool HasSelection => AssetBrowser.NumEntitiesSelected > 0;

	public IEnumerable<KeyValuePair<string, InstanceType>> SelectedEntities
	{
		get
		{
			foreach (InstanceEntityViewModel entityVM in AssetBrowser.SelectedEntities)
			{
				yield return new KeyValuePair<string, InstanceType>(entityVM.Name, entityVM.InstanceType);
			}
		}
	}

	public InstanceType SelectedType => AssetBrowser.SelectedEntity.InstanceType;

	public string SelectedName => AssetBrowser.SelectedEntity.Name;

	public IEnumerable<string> SelectedPaths
	{
		get
		{
			foreach (InstanceEntityViewModel entityVM in AssetBrowser.SelectedEntities)
			{
				yield return CivTechService.GetEntityPath(entityVM.Name, entityVM.InstanceType);
			}
		}
	}

	public string SelectedPath => CivTechService.GetEntityPath(AssetBrowser.SelectedEntity.Name, AssetBrowser.SelectedEntity.InstanceType);

	public RelayCommand AcceptCommand
	{
		get
		{
			return _acceptCommand ?? (_acceptCommand = new RelayCommand(Accept));
		}
		protected set
		{
			if (_acceptCommand == value)
			{
				_acceptCommand = value;
				OnPropertyChanged("AcceptCommand");
			}
		}
	}

	public RelayCommand CancelCommand
	{
		get
		{
			return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel));
		}
		protected set
		{
			if (_cancelCommand == value)
			{
				_cancelCommand = value;
				OnPropertyChanged("CancelCommand");
			}
		}
	}

	public AssetBrowserDialogViewModel(IEntityFilteringContext filteringContext, bool allowMultipleSelection = false)
	{
		_civTechService = CivTechRegistry.CivTechService;
		_assetBrowser = new AssetBrowserViewModel(_civTechService, CivTechRegistry.EntityFilteringService, Accept, allowMultipleSelection);
		InitializeAssetBrowserVMState(filteringContext);
	}

	public AssetBrowserDialogViewModel(IEnumerable<InstanceType> instanceTypes, IEnumerable<string> allowedClasses, bool allowMultipleSelection = false)
		: this(CivTechRegistry.EntityFilteringService.GetFilteringContext(instanceTypes, allowedClasses), allowMultipleSelection)
	{
	}

	private void InitializeAssetBrowserVMState(IEntityFilteringContext filteringContext)
	{
		InstanceTypeFilter instanceTypeFilter = filteringContext.FilterDefinitions.OfType<InstanceTypeFilter>().FirstOrDefault();
		if (instanceTypeFilter != null && !instanceTypeFilter.SelectedInstanceTypes.Any() && instanceTypeFilter.ValidInstanceTypes.Contains(InstanceType.IT_ASSET))
		{
			instanceTypeFilter.SelectedInstanceTypes.Add(InstanceType.IT_ASSET);
		}
		ConstrainedFilterViewModel filterVM = new ConstrainedFilterViewModel(filteringContext);
		AssetBrowser.FilterVM = filterVM;
		AssetBrowser.AutoSelectFirstEntity = true;
	}

	public void Accept()
	{
		DialogResult = true;
	}

	private void Cancel()
	{
		DialogResult = false;
	}
}
