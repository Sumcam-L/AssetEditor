using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Firaxis.CivTech.AssetObjects;

public class FilterCollection_Any : IFilterCollection_Any, IFilterCollectionDefinition, IEntityFilterDefinition, IEntityFilter
{
	private IEnumerable<IEntityFilter> m_filters;

	public string FilterName => "Any";

	public ICollection<IEntityFilterDefinition> FilterDefinitions { get; private set; }

	private IEnumerable<IEntityFilter> Filters
	{
		get
		{
			if (m_filters == null)
			{
				List<IEntityFilter> list = new List<IEntityFilter>();
				list.AddRange(FilterDefinitions.Select((IEntityFilterDefinition x) => x.CreateFilter()));
				m_filters = list;
			}
			return m_filters;
		}
		set
		{
			m_filters = value;
		}
	}

	public FilterCollection_Any()
	{
		ObservableCollection<IEntityFilterDefinition> observableCollection = new ObservableCollection<IEntityFilterDefinition>();
		observableCollection.CollectionChanged += FilterDefinitionsChanged;
		FilterDefinitions = observableCollection;
	}

	private void FilterDefinitionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		Filters = null;
	}

	public int GetRanking()
	{
		return Filters.Sum((IEntityFilter x) => x.GetRanking());
	}

	public bool PassesFilter(EntityID entity)
	{
		return Filters.OrderBy((IEntityFilter x) => x.GetRanking()).Any((IEntityFilter x) => PassesFilter(entity));
	}

	public IEntityFilterDefinition DeepCopy()
	{
		return DeepCopyImpl();
	}

	public IEntityFilter CreateFilter()
	{
		return DeepCopyImpl();
	}

	private FilterCollection_Any DeepCopyImpl()
	{
		FilterCollection_Any filterCollection_Any = new FilterCollection_Any();
		foreach (IEntityFilterDefinition filterDefinition in FilterDefinitions)
		{
			filterCollection_Any.FilterDefinitions.Add(filterDefinition.DeepCopy());
		}
		return filterCollection_Any;
	}
}
