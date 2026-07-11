using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class MaterialViewModel : Notifier
{
	private DelegateCommand m_addTagCommand;

	private static ObservableCollection<string> m_availableMaterialClasses;

	private IMaterialInstance m_material;

	private IMaterialClass m_materialClass;

	private IParameterSetViewModel m_parametersViewModel;

	private string m_tagToAdd;

	public ObservableCollection<string> AvailableMaterialClasses => m_availableMaterialClasses ?? (m_availableMaterialClasses = new ObservableCollection<string>(global::DatabaseWrapper.DatabaseWrapper.GetClassNames<IMaterialClass>(CivTechRegistry.CivTechService.PrimaryProject.Name)));

	public bool HasMaterialClass => !string.IsNullOrWhiteSpace(Material.ClassName);

	public IInstanceSet InstanceSet { get; set; }

	public IMaterialInstance Material
	{
		get
		{
			return m_material;
		}
		private set
		{
			if (m_material != value)
			{
				m_material = value;
			}
		}
	}

	public string MaterialClass
	{
		get
		{
			return Material.ClassName;
		}
		set
		{
			if (!(Material.ClassName == value))
			{
				IMaterialClass materialClass = global::DatabaseWrapper.DatabaseWrapper.GetMaterialClass(CivTechRegistry.CivTechService.PrimaryProject.Name, value);
				if (materialClass != null)
				{
					Material.ClassName = value;
					StaticMethods.RepopulateInstanceCookParameters(Material, materialClass);
					SetupMaterialViewModels(materialClass);
				}
			}
		}
	}

	public IMaterialClass MaterialClassInstance
	{
		get
		{
			return m_materialClass;
		}
		set
		{
			m_materialClass = value;
		}
	}

	public string Name
	{
		get
		{
			return m_material.Name;
		}
		set
		{
			if (!(m_material.Name == value))
			{
				m_material.Name = value.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				OnPropertyChanged("Name");
			}
		}
	}

	public IParameterSetViewModel ParametersViewModel
	{
		get
		{
			return m_parametersViewModel;
		}
		set
		{
			m_parametersViewModel = value;
			OnPropertyChanged("ParametersViewModel");
		}
	}

	public IEnumerable<string> Tags => m_material.Tags;

	public string TagsAsString => m_material.FlattenTagsToString();

	public string TagToAdd
	{
		get
		{
			return m_tagToAdd;
		}
		set
		{
			if (!(m_tagToAdd == value))
			{
				m_tagToAdd = value;
				OnPropertyChanged("TagToAdd");
			}
		}
	}

	protected ICivTechService CivTechService { get; private set; }

	public ICommand AddTagCommand => m_addTagCommand ?? (m_addTagCommand = new DelegateCommand(AddTag));

	public ICommand SaveCommand => null;

	public ICommand ValidateCommand => null;

	public MaterialViewModel(ICivTechService civTechSvc, IMaterialInstance materialInstance, IInstanceSet instances)
	{
		InstanceSet = instances;
		CivTechService = civTechSvc;
		m_material = materialInstance;
		if (!string.IsNullOrWhiteSpace(materialInstance.ClassName))
		{
			MaterialClass = materialInstance.ClassName;
		}
	}

	public static MaterialViewModel Open(ICivTechService civTechSvc, IMaterialInstance inst, IInstanceSet instances)
	{
		MaterialViewModel materialViewModel = new MaterialViewModel(civTechSvc, inst, instances);
		materialViewModel.SetupMaterialViewModels(global::DatabaseWrapper.DatabaseWrapper.GetMaterialClass(CivTechRegistry.CivTechService.PrimaryProject.Name, materialViewModel.MaterialClass));
		return materialViewModel;
	}

	private void AddTag(object context)
	{
		if (!m_material.Tags.Contains(TagToAdd))
		{
			m_material.AddTag(TagToAdd);
			OnPropertyChanged("Tags");
			OnPropertyChanged("TagsAsString");
			TagToAdd = string.Empty;
		}
	}

	private void SetupMaterialViewModels(IClassEntity materialClass)
	{
		ParametersViewModel = new ParameterSetViewModel(CivTechService, m_material, materialClass.CookParameters, Material.CookParameters, InstanceSet);
		OnPropertyChanged("MaterialClass");
		OnPropertyChanged("HasMaterialClass");
	}
}
