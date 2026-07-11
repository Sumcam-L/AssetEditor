using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class EntityViewModel : Notifier
{
	private IInstanceEntity m_entity;

	private bool m_overrideExisting = false;

	public IInstanceEntity Entity
	{
		get
		{
			return m_entity;
		}
		private set
		{
			if (m_entity != value)
			{
				m_entity = value;
				OnPropertyChanged("Entity");
				OnPropertyChanged("EntityName");
			}
		}
	}

	public string EntityName
	{
		get
		{
			return (Entity == null) ? string.Empty : Entity.Name;
		}
		set
		{
			if (Entity != null && Entity.Name != value)
			{
				Entity.Name = value;
				OnPropertyChanged("EntityName");
			}
		}
	}

	public bool OverrideExisting
	{
		get
		{
			return m_overrideExisting;
		}
		set
		{
			if (m_overrideExisting != value)
			{
				m_overrideExisting = value;
				OnPropertyChanged("OverrideExisting");
			}
		}
	}

	public EntityViewModel(IInstanceEntity entity)
	{
		Entity = entity;
	}
}
