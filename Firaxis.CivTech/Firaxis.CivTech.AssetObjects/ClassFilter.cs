using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace Firaxis.CivTech.AssetObjects;

public class ClassFilter : IClassFilterDefinition, IEntityFilterDefinition, IEntityFilter, IEntityCacheServiceConsumer
{
	private Regex m_filterRegex;

	public string FilterName => "Class";

	public ICollection<string> ValidClassNames { get; private set; }

	public IEntityCacheService EntityCacheService { get; set; }

	private bool TriviallyPasses => FilterRegex == null;

	private Regex FilterRegex
	{
		get
		{
			if (m_filterRegex == null)
			{
				m_filterRegex = BuildRegex();
			}
			return m_filterRegex;
		}
		set
		{
			m_filterRegex = value;
		}
	}

	public ClassFilter()
	{
		((ObservableCollection<string>)(ValidClassNames = new ObservableCollection<string>())).CollectionChanged += HandleValidClassesChanged;
	}

	private void HandleValidClassesChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		FilterRegex = null;
	}

	public int GetRanking()
	{
		return (!TriviallyPasses) ? 4 : 0;
	}

	public bool PassesFilter(EntityID entityID)
	{
		if (TriviallyPasses)
		{
			return true;
		}
		IEnumerable<IEntityCacheData> cacheData = EntityCacheService.GetCacheData(entityID);
		return cacheData.Any((IEntityCacheData entity) => FilterRegex.Match(entity.Class).Success);
	}

	public IEntityFilterDefinition DeepCopy()
	{
		return DeepCopyImpl();
	}

	public IEntityFilter CreateFilter()
	{
		return DeepCopyImpl();
	}

	private ClassFilter DeepCopyImpl()
	{
		ClassFilter classFilter = new ClassFilter();
		classFilter.EntityCacheService = EntityCacheService;
		foreach (string validClassName in ValidClassNames)
		{
			classFilter.ValidClassNames.Add(validClassName);
		}
		return classFilter;
	}

	private Regex BuildRegex()
	{
		if (ValidClassNames.Count == 0)
		{
			return null;
		}
		string pattern = string.Join("|", ValidClassNames.Select((string filter) => $"\\A{filter}\\z"));
		return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}
