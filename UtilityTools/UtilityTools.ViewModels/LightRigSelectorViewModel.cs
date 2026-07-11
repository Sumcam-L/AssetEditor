using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels;

public class LightRigSelectorViewModel : Notifier
{
	private List<string> m_allowedLightRigClasses;

	private ObservableCollection<string> m_availableLightRigs;

	private string m_selectedLightRig;

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

	public bool PreviewerSupportsLightRigs => AllowedLightRigClasses.Count > 0;

	public string SelectedLightRig
	{
		get
		{
			return m_selectedLightRig;
		}
		set
		{
			if (m_selectedLightRig != value)
			{
				m_selectedLightRig = value;
				OnPropertyChanged("SelectedLightRig");
				OnSelectedLightRigChangedEvent(value);
			}
		}
	}

	public int SelectedLightRigIndex
	{
		get
		{
			return AvailableLightRigs.IndexOf(SelectedLightRig);
		}
		set
		{
			if (SelectedLightRigIndex != value)
			{
				SelectedLightRig = AvailableLightRigs[value];
				OnPropertyChanged("SelectedLightRigIndex");
			}
		}
	}

	private List<string> AllowedLightRigClasses
	{
		get
		{
			return m_allowedLightRigClasses;
		}
		set
		{
			if (m_allowedLightRigClasses != value)
			{
				m_allowedLightRigClasses = value;
				OnPropertyChanged("PreviewerSupportsLightRigs");
			}
		}
	}

	private IInstanceSet Instances { get; set; }

	public event EventHandler<SelectedLightRigChangedEventArgs> SelectedLightRigChangedEvent;

	public LightRigSelectorViewModel(IInstanceSet instances)
	{
		Instances = instances;
		AllowedLightRigClasses = new List<string>();
		AvailableLightRigs = new ObservableCollection<string>();
	}

	public LightRigSelectorViewModel(IInstanceSet instances, IEnumerable<string> allowedLightRigClasses)
	{
		Instances = instances;
		AvailableLightRigs = new ObservableCollection<string>();
		UpdateAllowedLightRigClasses(allowedLightRigClasses, string.Empty);
	}

	public void UpdateAllowedLightRigClasses(IEnumerable<string> allowedLightRigClasses, string selectedLightRig)
	{
		AllowedLightRigClasses = new List<string>(allowedLightRigClasses);
		UpdateAvailableLightRigs(selectedLightRig);
	}

	public void UpdateAvailableLightRigs(string selectedLightRig)
	{
		AvailableLightRigs.Clear();
		ICivTechService civTechService = CivTechRegistry.CivTechService;
		IQueryService queryService = CivTechRegistry.EntityQueryService.FindFilesByName(new string[1] { civTechService.AnyProject }, string.Empty, AllowedLightRigClasses, new InstanceType[1] { InstanceType.IT_LIGHT_RIG });
		queryService.InstanceItems[InstanceType.IT_LIGHT_RIG].ForEach(delegate(string name)
		{
			AvailableLightRigs.Add(name);
		});
		SelectedLightRig = selectedLightRig;
		OnPropertyChanged("AvailableLightRigs");
		OnPropertyChanged("SelectedLightRigIndex");
		OnPropertyChanged("PreviewerSupportsLightRigs");
	}

	protected virtual void OnSelectedLightRigChangedEvent(string lightRigName)
	{
		if (PreviewerSupportsLightRigs && EngineContextWrapper.GetInstanceByNameAndType(InstanceType.IT_LIGHT_RIG, lightRigName, Instances) is ILightRigInstance lightRig)
		{
			this.SelectedLightRigChangedEvent?.Invoke(this, new SelectedLightRigChangedEventArgs(lightRig));
		}
	}
}
