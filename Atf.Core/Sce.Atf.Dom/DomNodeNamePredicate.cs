using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Sce.Atf.Dom;

public class DomNodeNamePredicate : IQueryPredicate
{
	private string m_stringToMatch;

	public string StringToMatch
	{
		get
		{
			return m_stringToMatch;
		}
		set
		{
			m_stringToMatch = value;
		}
	}

	public DomNodeNamePredicate()
	{
		StringToMatch = "";
	}

	internal static PropertyDescriptorCollection GetDomNodeProperties(DomNode node)
	{
		if (node == null)
		{
			return null;
		}
		if (!(node.GetAdapter(typeof(CustomTypeDescriptorNodeAdapter)) is ICustomTypeDescriptor customTypeDescriptor))
		{
			return null;
		}
		return customTypeDescriptor.GetProperties();
	}

	internal static string GetDomNodeName(DomNode domNode, out PropertyDescriptor nameProperty)
	{
		string result = "<UNKNOWN>";
		nameProperty = null;
		PropertyDescriptorCollection domNodeProperties = GetDomNodeProperties(domNode);
		if (domNodeProperties != null)
		{
			foreach (PropertyDescriptor item in domNodeProperties)
			{
				if (item.Name.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
				{
					nameProperty = item;
					result = item.GetValue(domNode) as string;
					break;
				}
			}
		}
		return result;
	}

	public bool Test(object item, out IList<IQueryMatch> matchList)
	{
		if (!(item is DomNode domNode))
		{
			throw new InvalidOperationException("DomNodeSearchTextBox passed a test item that was not a DomNode");
		}
		matchList = new List<IQueryMatch>();
		PropertyDescriptor nameProperty;
		string domNodeName = GetDomNodeName(domNode, out nameProperty);
		if (StringToMatch != null && StringToMatch.Length > 0 && domNodeName.IndexOf(StringToMatch, StringComparison.InvariantCultureIgnoreCase) != -1)
		{
			matchList.Add(new DomNodePropertyMatch(nameProperty, domNode));
			return true;
		}
		return false;
	}

	public void Replace(IList<IQueryMatch> matchList, object replaceValue)
	{
		foreach (IQueryMatch match in matchList)
		{
			string input = match.GetValue().ToString();
			match.SetValue(Regex.Replace(input, StringToMatch, replaceValue.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
		}
	}
}
