using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels;

public class EntityNamerViewModel : DialogViewModel
{
	private DelegateCommand m_overrideAllCommand;

	private ObservableCollection<EntityViewModel> m_entities = new ObservableCollection<EntityViewModel>();

	public ObservableCollection<EntityViewModel> Entities
	{
		get
		{
			return m_entities;
		}
		set
		{
			if (m_entities != value)
			{
				m_entities = value;
				OnPropertyChanged("Entities");
			}
		}
	}

	public ICommand OverrideAllCommand
	{
		get
		{
			if (m_overrideAllCommand == null)
			{
				m_overrideAllCommand = new DelegateCommand(ExecuteOverrideAllCommand);
			}
			return m_overrideAllCommand;
		}
	}

	public EntityNamerViewModel(IEnumerable<IInstanceEntity> entities)
	{
		if (entities == null)
		{
			throw new ArgumentNullException("Cannot instantiate the EntityNamerViewModel with a null set of entities.");
		}
		foreach (IInstanceEntity entity in entities)
		{
			EntityViewModel item = new EntityViewModel(entity);
			Entities.Add(item);
		}
	}

	protected override void ExecuteOKCommand(object context)
	{
		bool flag = false;
		List<string> list = new List<string>();
		foreach (EntityViewModel entity in Entities)
		{
			if (!entity.OverrideExisting && !global::DatabaseWrapper.DatabaseWrapper.IsEntityNameAvailable(CivTechRegistry.CivTechService.PrimaryProject.Name, entity.Entity))
			{
				flag = true;
				list.Add(entity.EntityName);
			}
		}
		if (flag)
		{
			list.Insert(0, "The following entities still have name clashes and have not been marked for override:\n");
			DialogHelper.DisplayError(string.Join("\n\t", list), "Name clash exists.");
		}
		else
		{
			base.ExecuteOKCommand(context);
		}
	}

	protected virtual void ExecuteOverrideAllCommand(object context)
	{
		foreach (EntityViewModel entity in Entities)
		{
			entity.OverrideExisting = true;
		}
	}
}
