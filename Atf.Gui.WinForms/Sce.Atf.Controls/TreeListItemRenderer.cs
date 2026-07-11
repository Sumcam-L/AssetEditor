using System;
using System.Drawing;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls;

public class TreeListItemRenderer : TreeItemRenderer
{
	private readonly IItemView m_itemView;

	public IItemView ItemView => m_itemView;

	internal DataEditor TrackingEditor { get; set; }

	public TreeListItemRenderer(IItemView itemView)
	{
		m_itemView = itemView;
	}

	public override void DrawData(TreeControl.Node node, Graphics g, int x, int y)
	{
		TreeListControl treeListControl = node.TreeControl as TreeListControl;
		ItemInfo itemInfo = new WinFormsItemInfo();
		m_itemView.GetInfo(node.Tag, itemInfo);
		if (itemInfo.Properties.Length == 0)
		{
			return;
		}
		Region clip = g.Clip;
		UpdateColumnWidths(node, itemInfo, g);
		int num = treeListControl.TreeWidth;
		for (int i = 0; i < itemInfo.Properties.Length; i++)
		{
			DataEditor dataEditor = itemInfo.Properties[i] as DataEditor;
			if (dataEditor != null)
			{
				if (TrackingEditor != null && TrackingEditor.Owner == node.Tag && TrackingEditor.Name == dataEditor.Name)
				{
					dataEditor = TrackingEditor;
				}
				Rectangle rectangle = new Rectangle(num, y, treeListControl.Columns[i].ActualWidth, treeListControl.GetRowHeight(node));
				if (i == itemInfo.Properties.Length - 1)
				{
					rectangle.Width = node.TreeControl.ActualClientSize.Width - num;
				}
				g.SetClip(rectangle);
				dataEditor.PaintValue(g, rectangle);
			}
			num += treeListControl.Columns[i].ActualWidth;
		}
		g.Clip = clip;
	}

	private void UpdateColumnWidths(TreeControl.Node node, ItemInfo info, Graphics g)
	{
		TreeListControl treeListControl = node.TreeControl as TreeListControl;
		if (!treeListControl.AutoResizeColumns)
		{
			return;
		}
		bool flag = treeListControl.Columns.Count == info.Properties.Length;
		for (int i = 0; i < info.Properties.Length; i++)
		{
			if (info.Properties[i] is DataEditor dataEditor)
			{
				SizeF sizeF = dataEditor.Measure(g, SizeF.Empty);
				if (flag)
				{
					treeListControl.Columns[i].ActualWidth = Math.Max(treeListControl.Columns[i].Width, (int)sizeF.Width);
				}
			}
		}
	}
}
