using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class MaterialObjectParameterViewModel : ObjectParameterViewModel
{
	private ObservableCollection<string> m_materialNames;

	private string m_selectedMaterialName;

	public ObservableCollection<string> MaterialNames
	{
		get
		{
			return m_materialNames;
		}
		set
		{
			if (m_materialNames != value)
			{
				m_materialNames = value;
				OnPropertyChanged("MaterialNames");
			}
		}
	}

	public string SelectedMaterialName
	{
		get
		{
			return m_selectedMaterialName;
		}
		set
		{
			m_selectedMaterialName = value;
			OnPropertyChanged("SelectedMaterialName");
			base.ObjectValue = value;
		}
	}

	private IObjectValue DefaultValue => ObjectParam.DefaultValue as IObjectValue;

	private IObjectParameter ObjectParam => (IObjectParameter)base.Parameter;

	private IObjectValue ObjectValueInternal => (IObjectValue)base.Value;

	public MaterialObjectParameterViewModel(ICivTechService civTechSvc, IObjectParameter param, IObjectValue value, IInstanceSet instances, IEnumerable<string> materialNames)
		: base(civTechSvc, param, value, instances)
	{
		MaterialNames = PopulateMaterialNamesList(materialNames);
		SelectedMaterialName = ObjectValueInternal.GetBoundObjectName();
	}

	public void AssignDefaultMaterial(string materialName, string stateName)
	{
		if (string.IsNullOrEmpty(SelectedMaterialName))
		{
			using (IInstanceSet instances = base.CivTechService.CivTechContext.CreateInstance<IInstanceSet>())
			{
				SelectedMaterialName = ObjectValueInternal.AssignDefaultMaterial(CivTechRegistry.CivTechService.ProjectMapService.LayeredPantry, ObjectParam, materialName, stateName, MaterialNames, instances, base.CivTechService.PrimaryProject.Paths.GamePantry);
			}
		}
	}

	protected override void OnObjectParameterChanged(string newObjectParameter)
	{
		if (!string.IsNullOrEmpty(newObjectParameter) && !MaterialNames.Contains(newObjectParameter))
		{
			MaterialNames.Add(newObjectParameter);
		}
		if (SelectedMaterialName != newObjectParameter)
		{
			SelectedMaterialName = newObjectParameter;
		}
	}

	private ObservableCollection<string> PopulateMaterialNamesList(IEnumerable<string> materialNames)
	{
		ObservableCollection<string> observableCollection = new ObservableCollection<string>();
		if (!string.IsNullOrEmpty(DefaultValue.GetBoundObjectName()))
		{
			observableCollection.Add(DefaultValue.GetBoundObjectName());
		}
		if (materialNames != null && materialNames.Any())
		{
			foreach (string materialName in materialNames)
			{
				if (EngineContextWrapper.GetInstanceByNameAndType(InstanceType.IT_MATERIAL, materialName, base.InstanceSet) is IMaterialInstance materialInstance && ObjectParam.AllowedClasses.Contains(materialInstance.ClassName))
				{
					observableCollection.Add(materialInstance.Name);
				}
			}
		}
		return observableCollection;
	}
}
