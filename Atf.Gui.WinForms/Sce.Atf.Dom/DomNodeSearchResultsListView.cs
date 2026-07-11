using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class DomNodeSearchResultsListView : SearchResultsListView
{
	private class HeaderData
	{
		private readonly string m_name;

		private string m_longestString;

		private readonly HorizontalAlignment m_horizontalAlignment;

		public string Name => m_name;

		public string LongestString => m_longestString;

		public HorizontalAlignment HorizontalAlignment => m_horizontalAlignment;

		private HeaderData()
		{
		}

		public HeaderData(string name, HorizontalAlignment horizontalAlignment)
		{
			m_name = name;
			m_longestString = name;
			m_horizontalAlignment = horizontalAlignment;
		}

		public void RegisterColumnString(string columnString)
		{
			if (columnString.Length > m_longestString.Length)
			{
				m_longestString = columnString;
			}
		}
	}

	public override event EventHandler UIChanged;

	public DomNodeSearchResultsListView()
		: base(null)
	{
		base.Columns.Add("Node Name", -2, HorizontalAlignment.Center);
		base.Columns.Add("Type", -2, HorizontalAlignment.Center);
		base.Columns.Add("Property", -2, HorizontalAlignment.Right);
		base.Columns.Add("Value", -2, HorizontalAlignment.Left);
		if (UIChanged != null)
		{
		}
	}

	public DomNodeSearchResultsListView(IContextRegistry contextRegistry)
		: base(contextRegistry)
	{
		base.Columns.Add("Node Name", -2, HorizontalAlignment.Center);
		base.Columns.Add("Type", -2, HorizontalAlignment.Center);
		base.Columns.Add("Property", -2, HorizontalAlignment.Right);
		base.Columns.Add("Value", -2, HorizontalAlignment.Left);
		if (UIChanged != null)
		{
		}
	}

	protected override void ClearResults()
	{
		foreach (ListViewItem item in base.Items)
		{
			DomNode domNode = (DomNode)item.Tag;
			if (domNode != null)
			{
				domNode.AttributeChanged -= DomNode_Changed;
			}
		}
		base.Columns.Clear();
		base.Items.Clear();
	}

	protected override void UpdateResults()
	{
		ClearResults();
		List<HeaderData> list = new List<HeaderData>();
		List<ListViewItem> list2 = new List<ListViewItem>();
		foreach (object result in base.QueryResultContext.Results)
		{
			if (!(result is DomNodeQueryMatch domNodeQueryMatch))
			{
				throw new InvalidOperationException("The class implementing IQueryableContext, which produced the results passed in, did not create results of type DomNodeQueryMatch.  Consider use DomNodeQueryable to create your search results.");
			}
			int num = 0;
			if (list.Count <= num)
			{
				list.Add(new HeaderData("Node Name", HorizontalAlignment.Left));
				list.Add(new HeaderData("Type", HorizontalAlignment.Center));
			}
			string domNodeName = GetDomNodeName(domNodeQueryMatch.DomNode);
			string text = domNodeQueryMatch.DomNode.Type.ToString();
			domNodeQueryMatch.DomNode.AttributeChanged += DomNode_Changed;
			ListViewItem listViewItem = new ListViewItem(new string[2] { domNodeName, text });
			listViewItem.Tag = domNodeQueryMatch.DomNode;
			list[num++].RegisterColumnString(domNodeName);
			list[num++].RegisterColumnString(text);
			foreach (IList<IQueryMatch> value in domNodeQueryMatch.PredicateMatchResults.Values)
			{
				if (!(value is List<IQueryMatch> list3))
				{
					continue;
				}
				foreach (DomNodePropertyMatch item in list3)
				{
					if (list.Count <= num)
					{
						list.Add(new HeaderData("Property", HorizontalAlignment.Right));
						list.Add(new HeaderData("Value", HorizontalAlignment.Left));
					}
					string name = item.Name;
					string columnString = item.GetValue().ToString();
					ListViewItem.ListViewSubItem listViewSubItem = new ListViewItem.ListViewSubItem(listViewItem, name);
					ListViewItem.ListViewSubItem listViewSubItem2 = new ListViewItem.ListViewSubItem(listViewItem, columnString);
					listViewSubItem.Tag = item.PropertyDescriptor;
					listViewSubItem2.Tag = item.PropertyDescriptor;
					listViewItem.SubItems.Add(listViewSubItem);
					listViewItem.SubItems.Add(listViewSubItem2);
					list[num++].RegisterColumnString(name);
					list[num++].RegisterColumnString(columnString);
				}
			}
			list2.Add(listViewItem);
		}
		Graphics graphics = CreateGraphics();
		foreach (HeaderData item2 in list)
		{
			int num2 = graphics.MeasureString(item2.LongestString, Font).ToSize().Width + 15;
			base.Columns.Add(item2.Name, num2, item2.HorizontalAlignment);
		}
		base.Items.AddRange(list2.ToArray());
	}

	private void DomNode_Changed(object sender, EventArgs e)
	{
		UpdateResults();
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

	internal static string GetDomNodeName(DomNode domNode)
	{
		string result = "<UNKNOWN>";
		PropertyDescriptorCollection domNodeProperties = GetDomNodeProperties(domNode);
		if (domNodeProperties != null)
		{
			foreach (System.ComponentModel.PropertyDescriptor item in domNodeProperties)
			{
				if (item.Name.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
				{
					result = item.GetValue(domNode) as string;
					break;
				}
			}
		}
		return result;
	}
}
