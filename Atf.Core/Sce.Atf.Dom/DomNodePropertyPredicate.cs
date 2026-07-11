using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Sce.Atf.Dom;

public class DomNodePropertyPredicate : LinqQueryPredicate
{
	private class PropertyDescriptorQueryable : IEnumerable<DomNodePropertyMatch>, IEnumerable
	{
		private readonly PropertyDescriptorCollection m_propertyDescriptorCollection;

		private readonly DomNode m_domNode;

		public PropertyDescriptorQueryable(PropertyDescriptorCollection propertyDescriptorCollection, DomNode domNode)
		{
			m_propertyDescriptorCollection = propertyDescriptorCollection;
			m_domNode = domNode;
		}

		public IEnumerator<DomNodePropertyMatch> GetEnumerator()
		{
			if (m_propertyDescriptorCollection == null)
			{
				yield break;
			}
			foreach (PropertyDescriptor pd in m_propertyDescriptorCollection)
			{
				yield return new DomNodePropertyMatch(pd, m_domNode);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private DomNode m_domNode;

	public MemberExpression QueryableName => Expression.PropertyOrField(m_queryableData, "Name");

	protected DomNode DomNode => m_domNode;

	public DomNodePropertyPredicate()
	{
		m_domNode = null;
		m_queryableData = Expression.Parameter(typeof(DomNodePropertyMatch), "queryableData");
	}

	public void AddPropertyNameExpression(string matchString)
	{
		MethodCallExpression stringIndexOfExpression = GetStringIndexOfExpression(QueryableName, matchString);
		AddExpression(Expression.OrElse(GetNullOrEmptyExpression(matchString), Expression.NotEqual(stringIndexOfExpression, Expression.Constant(-1))));
	}

	public void AddNameStringSearchExpression(string matchString, ulong searchType, bool isReplacePattern)
	{
		AddStringSearchExpression(QueryableName, matchString, searchType, isReplacePattern);
	}

	public override IQueryMatch CreatePredicateMatch(object searchItem, object queryMatch)
	{
		return (DomNodePropertyMatch)queryMatch;
	}

	protected override IQueryable GetQueryableElements(object item)
	{
		IQueryable result = null;
		if (item is DomNode domNode)
		{
			m_domNode = domNode;
			if (m_domNode.GetAdapter(typeof(ICustomTypeDescriptor)) is ICustomTypeDescriptor customTypeDescriptor)
			{
				PropertyDescriptorCollection properties = customTypeDescriptor.GetProperties();
				result = new PropertyDescriptorQueryable(properties, m_domNode).AsQueryable();
			}
		}
		return result;
	}
}
