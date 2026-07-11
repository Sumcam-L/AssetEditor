using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sce.Atf.Wpf.Docking;

internal class GridLayout : Grid, IDockLayout, IXmlSerializable
{
	private Size m_minGridSize = new Size(40.0, 20.0);

	private List<IDockLayout> m_children;

	private Orientation m_orientation;

	public List<IDockLayout> Layouts => m_children;

	public Orientation Orientation => m_orientation;

	public DockPanel Root { get; private set; }

	public GridLayout(DockPanel dockPanel)
	{
		Root = dockPanel;
		Panel.SetZIndex(this, 0);
		m_children = new List<IDockLayout>();
	}

	public GridLayout(DockPanel dockPanel, IDockLayout child)
		: this(dockPanel)
	{
		AddFirstChild(child);
	}

	public GridLayout(DockPanel dockPanel, XmlReader reader)
		: this(dockPanel)
	{
		ReadXml(reader);
	}

	private void AddFirstChild(IDockLayout child)
	{
		m_children.Add(child);
		base.Children.Add((FrameworkElement)child);
		base.ColumnDefinitions.Add(NewColumnDefinition(new GridLength(1.0, GridUnitType.Star), m_minGridSize.Width));
		base.RowDefinitions.Add(NewRowDefinition(new GridLength(1.0, GridUnitType.Star), m_minGridSize.Height));
		((FrameworkElement)child).SetValue(Grid.ColumnProperty, 0);
		((FrameworkElement)child).SetValue(Grid.RowProperty, 0);
	}

	private RowDefinition NewRowDefinition(GridLength length, double minHeight)
	{
		RowDefinition rowDefinition = new RowDefinition();
		rowDefinition.Height = length;
		rowDefinition.MinHeight = minHeight;
		return rowDefinition;
	}

	private ColumnDefinition NewColumnDefinition(GridLength length, double minWidth)
	{
		ColumnDefinition columnDefinition = new ColumnDefinition();
		columnDefinition.Width = length;
		columnDefinition.MinWidth = minWidth;
		return columnDefinition;
	}

	private GridSplitter NewGridSplitter(Orientation orientation)
	{
		GridSplitter gridSplitter = new GridSplitter();
		gridSplitter.Style = TryFindResource(DockPanel.GridSplitterStyleKey) as Style;
		gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
		gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
		gridSplitter.SnapsToDevicePixels = true;
		gridSplitter.Focusable = false;
		gridSplitter.ResizeDirection = ((orientation == Orientation.Horizontal) ? GridResizeDirection.Columns : GridResizeDirection.Rows);
		return gridSplitter;
	}

	internal void MergeWith(GridLayout grid)
	{
		int num = m_children.IndexOf(grid);
		double num2 = 0.0;
		int num3 = 0;
		if (Orientation == Orientation.Horizontal)
		{
			ColumnDefinition columnDefinition = base.ColumnDefinitions[num * 2];
			foreach (IDockLayout layout in grid.Layouts)
			{
				FrameworkElement element = (FrameworkElement)layout;
				int column = Grid.GetColumn(element);
				ColumnDefinition columnDefinition2 = grid.ColumnDefinitions[column];
				num2 += columnDefinition2.Width.Value;
			}
			m_children.RemoveAt(num);
			base.Children.RemoveAt(num * 2);
			base.ColumnDefinitions.RemoveAt(num * 2);
			foreach (IDockLayout layout2 in grid.Layouts)
			{
				FrameworkElement element2 = (FrameworkElement)layout2;
				int column2 = Grid.GetColumn(element2);
				ColumnDefinition columnDefinition3 = grid.ColumnDefinitions[column2];
				double value = columnDefinition.Width.Value * columnDefinition3.Width.Value / num2;
				ColumnDefinition value2 = NewColumnDefinition(new GridLength(value, GridUnitType.Star), m_minGridSize.Width);
				grid.Children.Remove(element2);
				base.ColumnDefinitions.Insert((num + num3) * 2, value2);
				base.Children.Insert((num + num3) * 2, element2);
				m_children.Insert(num + num3, layout2);
				if (num3 < grid.Layouts.Count - 1)
				{
					GridSplitter element3 = NewGridSplitter(Orientation.Horizontal);
					base.ColumnDefinitions.Insert((num + num3) * 2 + 1, NewColumnDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
					base.Children.Insert((num + num3) * 2 + 1, element3);
				}
				num3++;
			}
			for (int i = 0; i < base.Children.Count; i++)
			{
				Grid.SetColumn(base.Children[i], i);
			}
			return;
		}
		RowDefinition rowDefinition = base.RowDefinitions[num * 2];
		foreach (IDockLayout layout3 in grid.Layouts)
		{
			FrameworkElement element4 = (FrameworkElement)layout3;
			int row = Grid.GetRow(element4);
			RowDefinition rowDefinition2 = grid.RowDefinitions[row];
			num2 += rowDefinition2.Height.Value;
		}
		m_children.RemoveAt(num);
		base.Children.RemoveAt(num * 2);
		base.RowDefinitions.RemoveAt(num * 2);
		foreach (IDockLayout layout4 in grid.Layouts)
		{
			FrameworkElement element5 = (FrameworkElement)layout4;
			int row2 = Grid.GetRow(element5);
			RowDefinition rowDefinition3 = grid.RowDefinitions[row2];
			double value3 = rowDefinition.Height.Value * rowDefinition3.Height.Value / num2;
			RowDefinition value4 = NewRowDefinition(new GridLength(value3, GridUnitType.Star), m_minGridSize.Height);
			grid.Children.Remove(element5);
			base.RowDefinitions.Insert((num + num3) * 2, value4);
			base.Children.Insert((num + num3) * 2, element5);
			m_children.Insert(num + num3, layout4);
			if (num3 < grid.Layouts.Count - 1)
			{
				GridSplitter element6 = NewGridSplitter(Orientation.Vertical);
				base.RowDefinitions.Insert((num + num3) * 2 + 1, NewRowDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
				base.Children.Insert((num + num3) * 2 + 1, element6);
			}
			num3++;
		}
		for (int j = 0; j < base.Children.Count; j++)
		{
			Grid.SetRow(base.Children[j], j);
		}
	}

	public DockContent HitTest(Point position)
	{
		Rect rect = new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight);
		Point point = PointFromScreen(position);
		if (rect.Contains(point))
		{
			foreach (IDockLayout child in m_children)
			{
				DockContent dockContent = child.HitTest(position);
				if (dockContent != null)
				{
					return dockContent;
				}
			}
		}
		return null;
	}

	public bool HasChild(IDockContent content)
	{
		return false;
	}

	public bool HasDescendant(IDockContent content)
	{
		foreach (IDockLayout child in m_children)
		{
			if (child.HasDescendant(content))
			{
				return true;
			}
		}
		return false;
	}

	public void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
	{
		IDockLayout dockLayout = null;
		if (nextTo != null)
		{
			foreach (IDockLayout child in m_children)
			{
				if (child.HasChild(nextTo))
				{
					dockLayout = child;
					break;
				}
			}
		}
		if (dockLayout == null)
		{
			foreach (IDockLayout child2 in m_children)
			{
				if (child2.HasDescendant(nextTo))
				{
					child2.Dock(nextTo, newContent, dockTo);
					return;
				}
			}
		}
		if (dockTo == DockTo.Center && m_children.Count == 1)
		{
			m_children[0].Dock(null, newContent, dockTo);
		}
		else if (m_children.Count < 2)
		{
			if (dockTo == DockTo.Center)
			{
				dockTo = DockTo.Right;
			}
			if (dockTo == DockTo.Top || dockTo == DockTo.Bottom)
			{
				m_orientation = Orientation.Vertical;
			}
			else
			{
				m_orientation = Orientation.Horizontal;
			}
			DockedWindow dockedWindow = new DockedWindow(Root, newContent);
			if (base.Children.Count == 0)
			{
				AddFirstChild(dockedWindow);
				return;
			}
			if (dockLayout == null)
			{
				dockLayout = ((dockTo != DockTo.Top && dockTo != DockTo.Left) ? m_children[m_children.Count - 1] : m_children[0]);
			}
			FrameworkElement frameworkElement = (FrameworkElement)dockLayout;
			int num = m_children.IndexOf(dockLayout);
			if (dockTo == DockTo.Left || dockTo == DockTo.Right)
			{
				GridSplitter gridSplitter = NewGridSplitter(Orientation.Horizontal);
				int num2 = (int)frameworkElement.GetValue(Grid.ColumnProperty);
				ColumnDefinition columnDefinition = base.ColumnDefinitions[num * 2];
				ContentSettings contentSettings = ((newContent is TabLayout) ? ((TabLayout)newContent).Children[0].Settings : ((DockContent)newContent).Settings);
				double actualWidth = ((FrameworkElement)dockLayout).ActualWidth;
				double num3 = Math.Max(Math.Min(contentSettings.Size.Width, (actualWidth - gridSplitter.Width) / 2.0), (actualWidth - gridSplitter.Width) / 5.0);
				double num4 = num3 / actualWidth;
				double num5 = (actualWidth - num3 - gridSplitter.Width) / actualWidth;
				if (dockTo == DockTo.Left)
				{
					ColumnDefinition value = NewColumnDefinition(new GridLength(columnDefinition.Width.Value * num4, columnDefinition.Width.GridUnitType), m_minGridSize.Width);
					ColumnDefinition value2 = NewColumnDefinition(new GridLength(columnDefinition.Width.Value * num5, columnDefinition.Width.GridUnitType), m_minGridSize.Width);
					base.ColumnDefinitions[num * 2] = value;
					base.ColumnDefinitions.Insert(num * 2 + 1, value2);
					base.ColumnDefinitions.Insert(num * 2 + 1, NewColumnDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
					m_children.Insert(num, dockedWindow);
					base.Children.Insert(num * 2, gridSplitter);
					base.Children.Insert(num * 2, dockedWindow);
				}
				else
				{
					ColumnDefinition value3 = NewColumnDefinition(new GridLength(columnDefinition.Width.Value * num5, columnDefinition.Width.GridUnitType), m_minGridSize.Width);
					ColumnDefinition value4 = NewColumnDefinition(new GridLength(columnDefinition.Width.Value * num4, columnDefinition.Width.GridUnitType), m_minGridSize.Width);
					base.ColumnDefinitions[num * 2] = value3;
					base.ColumnDefinitions.Insert(num * 2 + 1, value4);
					base.ColumnDefinitions.Insert(num * 2 + 1, NewColumnDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
					m_children.Insert(num + 1, dockedWindow);
					base.Children.Insert(num * 2 + 1, dockedWindow);
					base.Children.Insert(num * 2 + 1, gridSplitter);
				}
				for (int i = num * 2; i < base.Children.Count; i++)
				{
					Grid.SetColumn(base.Children[i], i);
				}
			}
			else
			{
				GridSplitter gridSplitter2 = NewGridSplitter(Orientation.Vertical);
				int num6 = (int)frameworkElement.GetValue(Grid.RowProperty);
				RowDefinition rowDefinition = base.RowDefinitions[num * 2];
				ContentSettings contentSettings2 = ((newContent is TabLayout) ? ((TabLayout)newContent).Children[0].Settings : ((DockContent)newContent).Settings);
				double actualHeight = ((FrameworkElement)dockLayout).ActualHeight;
				double num7 = Math.Max(Math.Min(contentSettings2.Size.Height, (actualHeight - gridSplitter2.Height) / 2.0), (actualHeight - gridSplitter2.Height) / 5.0);
				double num8 = num7 / actualHeight;
				double num9 = (actualHeight - num7 - gridSplitter2.Height) / actualHeight;
				if (dockTo == DockTo.Top)
				{
					RowDefinition value5 = NewRowDefinition(new GridLength(rowDefinition.Height.Value * num8, rowDefinition.Height.GridUnitType), m_minGridSize.Height);
					RowDefinition value6 = NewRowDefinition(new GridLength(rowDefinition.Height.Value * num9, rowDefinition.Height.GridUnitType), m_minGridSize.Height);
					base.RowDefinitions[num * 2] = value5;
					base.RowDefinitions.Insert(num * 2 + 1, value6);
					base.RowDefinitions.Insert(num * 2 + 1, NewRowDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
					m_children.Insert(num, dockedWindow);
					base.Children.Insert(num * 2, gridSplitter2);
					base.Children.Insert(num * 2, dockedWindow);
				}
				else
				{
					RowDefinition value7 = NewRowDefinition(new GridLength(rowDefinition.Height.Value * num9, rowDefinition.Height.GridUnitType), m_minGridSize.Height);
					RowDefinition value8 = NewRowDefinition(new GridLength(rowDefinition.Height.Value * num8, rowDefinition.Height.GridUnitType), m_minGridSize.Height);
					base.RowDefinitions[num * 2] = value7;
					base.RowDefinitions.Insert(num * 2 + 1, value8);
					base.RowDefinitions.Insert(num * 2 + 1, NewRowDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
					m_children.Insert(num + 1, dockedWindow);
					base.Children.Insert(num * 2 + 1, dockedWindow);
					base.Children.Insert(num * 2 + 1, gridSplitter2);
				}
				for (int j = num * 2; j < base.Children.Count; j++)
				{
					Grid.SetRow(base.Children[j], j);
				}
			}
		}
		else if (dockTo == DockTo.Left || dockTo == DockTo.Right || dockTo == DockTo.Top || dockTo == DockTo.Bottom)
		{
			DockedWindow dockedWindow2 = (DockedWindow)dockLayout;
			int num10 = m_children.IndexOf(dockLayout);
			GridLayout gridLayout = new GridLayout(Root);
			gridLayout.SetValue(Grid.ColumnProperty, dockedWindow2.GetValue(Grid.ColumnProperty));
			gridLayout.SetValue(Grid.RowProperty, dockedWindow2.GetValue(Grid.RowProperty));
			base.Children.Remove(dockedWindow2);
			IDockContent dockedContent = dockedWindow2.DockedContent;
			dockedWindow2.Undock(dockedContent);
			m_children[num10] = gridLayout;
			base.Children.Insert(num10 * 2, gridLayout);
			gridLayout.Dock(null, dockedContent, DockTo.Center);
			UpdateLayout();
			gridLayout.Dock(dockedContent, newContent, dockTo);
		}
		else
		{
			dockLayout?.Dock(nextTo, newContent, dockTo);
		}
	}

	public void Undock(IDockContent content)
	{
		IDockLayout dockLayout = null;
		foreach (IDockLayout child in m_children)
		{
			if (child.HasChild(content))
			{
				dockLayout = child;
				break;
			}
		}
		if (dockLayout != null)
		{
			dockLayout.Undock(content);
			return;
		}
		foreach (IDockLayout child2 in m_children)
		{
			if (child2.HasDescendant(content))
			{
				dockLayout = child2;
				break;
			}
		}
		dockLayout?.Undock(content);
	}

	public void Undock(IDockLayout child)
	{
		int num = m_children.IndexOf(child);
		if (Orientation == Orientation.Horizontal)
		{
			base.ColumnDefinitions.RemoveAt(num * 2);
			base.Children.RemoveAt(num * 2);
			if (m_children.Count > 1)
			{
				base.ColumnDefinitions.RemoveAt(num * 2 + ((num != 0) ? (-1) : 0));
				base.Children.RemoveAt(num * 2 + ((num != 0) ? (-1) : 0));
				for (int i = 0; i < base.Children.Count; i++)
				{
					Grid.SetColumn(base.Children[i], i);
				}
			}
		}
		else
		{
			base.RowDefinitions.RemoveAt(num * 2);
			base.Children.RemoveAt(num * 2);
			if (m_children.Count > 1)
			{
				base.RowDefinitions.RemoveAt(num * 2 + ((num != 0) ? (-1) : 0));
				base.Children.RemoveAt(num * 2 + ((num != 0) ? (-1) : 0));
				for (int j = 0; j < base.Children.Count; j++)
				{
					Grid.SetRow(base.Children[j], j);
				}
			}
		}
		m_children.RemoveAt(num);
		Root.CheckConsistency();
	}

	public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
	{
		FrameworkElement frameworkElement = (FrameworkElement)newLayout;
		frameworkElement.SetValue(Grid.ColumnProperty, (oldLayout as FrameworkElement).GetValue(Grid.ColumnProperty));
		frameworkElement.SetValue(Grid.RowProperty, (oldLayout as FrameworkElement).GetValue(Grid.RowProperty));
		m_children[m_children.IndexOf(oldLayout)] = newLayout;
		int index = base.Children.IndexOf((FrameworkElement)oldLayout);
		base.Children.RemoveAt(index);
		base.Children.Insert(index, (FrameworkElement)newLayout);
	}

	internal bool CheckConsistency()
	{
		IEnumerator<IDockLayout> enumerator = m_children.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is GridLayout gridLayout && !gridLayout.CheckConsistency())
			{
				enumerator = m_children.GetEnumerator();
			}
		}
		if (m_children.Count == 0)
		{
			if (base.Parent != null)
			{
				((IDockLayout)base.Parent).Undock(this);
			}
			else
			{
				((IDockLayout)Root).Undock((IDockLayout)this);
			}
			return false;
		}
		if (m_children.Count == 1)
		{
			if (base.Parent is IDockLayout || m_children[0] != null)
			{
				IDockLayout dockLayout = m_children[0];
				if (!(dockLayout is DockedWindow) || base.Parent != null)
				{
					m_children.Clear();
					base.Children.Clear();
					if (base.Parent != null)
					{
						((IDockLayout)base.Parent).Replace(this, dockLayout);
					}
					else
					{
						((IDockLayout)Root).Replace((IDockLayout)this, dockLayout);
					}
					return false;
				}
			}
		}
		else if (base.Parent is GridLayout && Orientation == ((GridLayout)base.Parent).Orientation)
		{
			((GridLayout)base.Parent).MergeWith(this);
			return false;
		}
		return true;
	}

	public void Close()
	{
		foreach (IDockLayout child in m_children)
		{
			child.Close();
		}
		m_children.Clear();
		base.Children.Clear();
	}

	IDockLayout IDockLayout.FindParentLayout(IDockContent content)
	{
		foreach (IDockLayout child in m_children)
		{
			IDockLayout dockLayout = null;
			if ((dockLayout = child.FindParentLayout(content)) != null)
			{
				return dockLayout;
			}
		}
		return null;
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		if (!reader.ReadToFollowing(GetType().Name))
		{
			return;
		}
		string attribute = reader.GetAttribute(Orientation.GetType().Name);
		m_orientation = (Orientation)Enum.Parse(Orientation.GetType(), attribute);
		switch (m_orientation)
		{
		case Orientation.Horizontal:
			if (!reader.ReadToDescendant("Column"))
			{
				break;
			}
			base.RowDefinitions.Add(NewRowDefinition(new GridLength(1.0, GridUnitType.Star), m_minGridSize.Height));
			do
			{
				double value2 = double.Parse(reader.GetAttribute("Width"));
				IDockLayout dockLayout2 = null;
				reader.ReadStartElement();
				if (reader.LocalName == typeof(DockedWindow).Name)
				{
					DockedWindow dockedWindow2 = new DockedWindow(Root, reader.ReadSubtree());
					dockLayout2 = ((dockedWindow2.DockedContent.Children.Count != 0) ? dockedWindow2 : null);
					reader.ReadEndElement();
				}
				else if (reader.LocalName == typeof(GridLayout).Name)
				{
					GridLayout gridLayout2 = new GridLayout(Root, reader.ReadSubtree());
					dockLayout2 = ((gridLayout2.Layouts.Count > 0) ? gridLayout2 : null);
					reader.ReadEndElement();
				}
				if (dockLayout2 != null)
				{
					if (base.Children.Count > 0)
					{
						base.ColumnDefinitions.Add(NewColumnDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
						base.Children.Add(NewGridSplitter(Orientation));
					}
					base.ColumnDefinitions.Add(NewColumnDefinition(new GridLength(value2, GridUnitType.Star), m_minGridSize.Width));
					m_children.Add(dockLayout2);
					base.Children.Add((FrameworkElement)dockLayout2);
				}
			}
			while (reader.ReadToNextSibling("Column"));
			break;
		case Orientation.Vertical:
			if (!reader.ReadToDescendant("Row"))
			{
				break;
			}
			base.ColumnDefinitions.Add(NewColumnDefinition(new GridLength(1.0, GridUnitType.Star), m_minGridSize.Width));
			do
			{
				double value = double.Parse(reader.GetAttribute("Height"));
				IDockLayout dockLayout = null;
				reader.ReadStartElement();
				if (reader.LocalName == typeof(DockedWindow).Name)
				{
					DockedWindow dockedWindow = new DockedWindow(Root, reader.ReadSubtree());
					dockLayout = ((dockedWindow.DockedContent.Children.Count != 0) ? dockedWindow : null);
					reader.ReadEndElement();
				}
				else if (reader.LocalName == typeof(GridLayout).Name)
				{
					GridLayout gridLayout = new GridLayout(Root, reader.ReadSubtree());
					dockLayout = ((gridLayout.Layouts.Count > 0) ? gridLayout : null);
					reader.ReadEndElement();
				}
				if (dockLayout != null)
				{
					if (base.Children.Count > 0)
					{
						base.RowDefinitions.Add(NewRowDefinition(new GridLength(1.0, GridUnitType.Auto), 0.0));
						base.Children.Add(NewGridSplitter(Orientation));
					}
					base.RowDefinitions.Add(NewRowDefinition(new GridLength(value, GridUnitType.Star), m_minGridSize.Height));
					m_children.Add(dockLayout);
					base.Children.Add((FrameworkElement)dockLayout);
				}
			}
			while (reader.ReadToNextSibling("Row"));
			break;
		}
		for (int i = 0; i < base.Children.Count; i++)
		{
			Grid.SetColumn(base.Children[i], (Orientation == Orientation.Horizontal) ? i : 0);
			Grid.SetRow(base.Children[i], (Orientation == Orientation.Vertical) ? i : 0);
		}
		reader.ReadEndElement();
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteStartElement(GetType().Name);
		writer.WriteAttributeString(Orientation.GetType().Name, Orientation.ToString());
		switch (Orientation)
		{
		case Orientation.Horizontal:
			foreach (IXmlSerializable child in m_children)
			{
				writer.WriteStartElement("Column");
				writer.WriteAttributeString("Width", base.ColumnDefinitions[Grid.GetColumn((FrameworkElement)child)].Width.Value.ToString());
				child.WriteXml(writer);
				writer.WriteEndElement();
			}
			break;
		case Orientation.Vertical:
			foreach (IXmlSerializable child2 in m_children)
			{
				writer.WriteStartElement("Row");
				writer.WriteAttributeString("Height", base.RowDefinitions[Grid.GetRow((FrameworkElement)child2)].Height.Value.ToString());
				child2.WriteXml(writer);
				writer.WriteEndElement();
			}
			break;
		}
		writer.WriteEndElement();
	}
}
