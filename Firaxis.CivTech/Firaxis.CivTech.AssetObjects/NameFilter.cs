using System.Text.RegularExpressions;

namespace Firaxis.CivTech.AssetObjects;

public class NameFilter : INameFilterDefinition, ITextInputFilter, IEntityFilterDefinition, IEntityFilter
{
	private string m_filterText;

	private Regex m_filterRegex;

	public string FilterName => "Name";

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
		return (!TriviallyPasses) ? 2 : 0;
	}

	public bool PassesFilter(EntityID entity)
	{
		return TriviallyPasses || FilterRegex.Match(entity.Name).Success;
	}

	public IEntityFilterDefinition DeepCopy()
	{
		return DeepCopyImpl();
	}

	public IEntityFilter CreateFilter()
	{
		return DeepCopyImpl();
	}

	private NameFilter DeepCopyImpl()
	{
		NameFilter nameFilter = new NameFilter();
		nameFilter.FilterText = FilterText;
		return nameFilter;
	}
}
