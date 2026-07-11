using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Firaxis.Controls;

public static class PropertyGridExtensions
{
	public static void SelectGridItem(this PropertyGrid grid, int index)
	{
		GridItem selectedGridItem = grid.SelectedGridItem;
		if (selectedGridItem != null)
		{
			GridItem gridItem = selectedGridItem;
			while (gridItem.GridItemType != GridItemType.Root)
			{
				gridItem = gridItem.Parent;
			}
			List<GridItem> gridItems = new List<GridItem>();
			grid.FindGridItems(gridItem, ref gridItems);
			if (index < 0 || index >= gridItems.Count)
			{
				throw new IndexOutOfRangeException($"Index {index} is out of range for the property grid's items [0, {gridItems.Count - 1}]");
			}
			grid.SelectedGridItem = gridItems[index];
		}
	}

	public static void SelectGridItem(this PropertyGrid grid, string label)
	{
		GridItem selectedGridItem = grid.SelectedGridItem;
		if (selectedGridItem == null)
		{
			return;
		}
		GridItem gridItem = selectedGridItem;
		while (gridItem.GridItemType != GridItemType.Root)
		{
			gridItem = gridItem.Parent;
		}
		List<GridItem> gridItems = new List<GridItem>();
		grid.FindGridItems(gridItem, ref gridItems);
		int num = 0;
		foreach (GridItem item in gridItems)
		{
			if (item.Label == label)
			{
				break;
			}
			num++;
		}
		if (num < 0 || num >= gridItems.Count)
		{
			throw new IndexOutOfRangeException($"Label \"{label}\" not found among the property grid's items.");
		}
		grid.SelectedGridItem = gridItems[num];
	}

	public static void SelectNextGridItem(this PropertyGrid grid)
	{
		GridItem selectedGridItem = grid.SelectedGridItem;
		if (selectedGridItem != null)
		{
			GridItem gridItem = selectedGridItem;
			while (gridItem.GridItemType != GridItemType.Root)
			{
				gridItem = gridItem.Parent;
			}
			List<GridItem> gridItems = new List<GridItem>();
			grid.FindGridItems(gridItem, ref gridItems);
			int num = gridItems.IndexOf(selectedGridItem) + 1;
			grid.SelectedGridItem = gridItems[num % gridItems.Count];
		}
	}

	public static void SelectPreviousGridItem(this PropertyGrid grid)
	{
		GridItem selectedGridItem = grid.SelectedGridItem;
		if (selectedGridItem != null)
		{
			GridItem gridItem = selectedGridItem;
			while (gridItem.GridItemType != GridItemType.Root)
			{
				gridItem = gridItem.Parent;
			}
			List<GridItem> gridItems = new List<GridItem>();
			grid.FindGridItems(gridItem, ref gridItems);
			int num = gridItems.IndexOf(selectedGridItem) - 1;
			if (num < 0)
			{
				num = gridItems.Count - 1;
			}
			grid.SelectedGridItem = gridItems[num];
		}
	}

	public static int GridItemsCount(this PropertyGrid grid)
	{
		GridItem selectedGridItem = grid.SelectedGridItem;
		if (selectedGridItem == null)
		{
			return 0;
		}
		GridItem gridItem = selectedGridItem;
		while (gridItem.GridItemType != GridItemType.Root)
		{
			gridItem = gridItem.Parent;
		}
		List<GridItem> gridItems = new List<GridItem>();
		grid.FindGridItems(gridItem, ref gridItems);
		return gridItems.Count;
	}

	public static void FindGridItems(this PropertyGrid grid, GridItem item, ref List<GridItem> gridItems)
	{
		switch (item.GridItemType)
		{
		case GridItemType.Category:
		case GridItemType.Root:
		{
			foreach (GridItem gridItem in item.GridItems)
			{
				grid.FindGridItems(gridItem, ref gridItems);
			}
			break;
		}
		case GridItemType.Property:
			gridItems.Add(item);
			if (!item.Expanded)
			{
				break;
			}
			{
				foreach (GridItem gridItem2 in item.GridItems)
				{
					grid.FindGridItems(gridItem2, ref gridItems);
				}
				break;
			}
		case GridItemType.ArrayValue:
			break;
		}
	}
}
