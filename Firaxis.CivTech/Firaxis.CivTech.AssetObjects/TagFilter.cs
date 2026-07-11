using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Firaxis.CivTech.AssetObjects;

public class TagFilter : ITagFilterDefinition, ITextInputFilter, IEntityFilterDefinition, IEntityFilter, IEntityCacheServiceConsumer
{
	private string m_filterText;

	private Regex m_filterRegex;

	public string FilterName => "Tag";

	public string FilterText
	{
		get
		{
			return m_filterText;
		}
		set
		{
			if (m_filterText != value)
			{
				m_filterText = value;
				FilterRegex = null;
			}
		}
	}

	public IEntityCacheService EntityCacheService { get; set; }

	private bool TriviallyPasses => FilterRegex == null;

	private Regex FilterRegex
	{
		get
		{
			if (m_filterRegex == null)
			{
				m_filterRegex = RegexFilterHelper.BuildRegex(FilterText);
			}
			return m_filterRegex;
		}
		set
		{
			m_filterRegex = value;
		}
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
		return cacheData.Any((IEntityCacheData entity) => entity.Tags.Any((string tag) => FilterRegex.Match(tag).Success));
	}

	public IEntityFilterDefinition DeepCopy()
	{
		return DeepCopyImpl();
	}

	public IEntityFilter CreateFilter()
	{
		return DeepCopyImpl();
	}

	private TagFilter DeepCopyImpl()
	{
		TagFilter tagFilter = new TagFilter();
		tagFilter.FilterText = FilterText;
		tagFilter.EntityCacheService = EntityCacheService;
		return tagFilter;
	}
}
