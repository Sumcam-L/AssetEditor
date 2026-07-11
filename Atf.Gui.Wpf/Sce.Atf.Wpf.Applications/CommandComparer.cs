using System;
using System.Collections.Generic;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public class CommandComparer : IComparer<ICommandItem>, IComparer<IMenuItem>
{
	private static readonly HashSet<object> m_beginningTags;

	private static readonly HashSet<object> m_endingTags;

	private static readonly HashSet<object> m_defaultSortByMenuLabel;

	static CommandComparer()
	{
		m_beginningTags = new HashSet<object>();
		m_endingTags = new HashSet<object>();
		m_defaultSortByMenuLabel = new HashSet<object>();
		m_endingTags.Add(StandardCommand.FileClose);
		m_beginningTags.Add(StandardCommand.FileSave);
		m_beginningTags.Add(StandardCommand.FileSaveAs);
		m_beginningTags.Add(StandardCommand.FileSaveAll);
		m_beginningTags.Add(StandardCommand.EditUndo);
		m_beginningTags.Add(StandardCommand.EditRedo);
		m_beginningTags.Add(StandardCommand.EditCut);
		m_beginningTags.Add(StandardCommand.EditCopy);
		m_beginningTags.Add(StandardCommand.EditPaste);
		m_endingTags.Add(StandardCommand.EditDelete);
		m_beginningTags.Add(StandardCommand.EditSelectAll);
		m_beginningTags.Add(StandardCommand.EditDeselectAll);
		m_beginningTags.Add(StandardCommand.EditInvertSelection);
		m_beginningTags.Add(StandardCommand.EditGroup);
		m_beginningTags.Add(StandardCommand.EditUngroup);
		m_beginningTags.Add(StandardCommand.EditLock);
		m_beginningTags.Add(StandardCommand.EditUnlock);
		m_beginningTags.Add(StandardCommand.ViewZoomIn);
		m_beginningTags.Add(StandardCommand.ViewZoomOut);
		m_beginningTags.Add(StandardCommand.ViewZoomExtents);
		m_beginningTags.Add(StandardCommand.WindowSplitHoriz);
		m_beginningTags.Add(StandardCommand.WindowSplitVert);
		m_beginningTags.Add(StandardCommand.WindowRemoveSplit);
		m_endingTags.Add(StandardCommand.HelpAbout);
		m_beginningTags.Add(StandardCommandGroup.FileNew);
		m_beginningTags.Add(StandardCommandGroup.FileSave);
		m_beginningTags.Add(StandardCommandGroup.FileOther);
		m_endingTags.Add(StandardCommandGroup.FileRecentlyUsed);
		m_endingTags.Add(StandardCommandGroup.FileExit);
		m_beginningTags.Add(StandardCommandGroup.EditUndo);
		m_beginningTags.Add(StandardCommandGroup.EditCut);
		m_beginningTags.Add(StandardCommandGroup.EditSelectAll);
		m_beginningTags.Add(StandardCommandGroup.EditGroup);
		m_beginningTags.Add(StandardCommandGroup.EditOther);
		m_endingTags.Add(StandardCommandGroup.EditPreferences);
		m_beginningTags.Add(StandardCommandGroup.ViewZoomIn);
		m_beginningTags.Add(StandardCommandGroup.ViewControls);
		m_beginningTags.Add(StandardCommandGroup.WindowLayout);
		m_beginningTags.Add(StandardCommandGroup.WindowSplit);
		m_endingTags.Add(StandardCommandGroup.WindowDocuments);
		m_endingTags.Add(StandardCommandGroup.HelpAbout);
		m_beginningTags.Add(CommandId.FileRecentlyUsed1);
		m_beginningTags.Add(CommandId.FileRecentlyUsed2);
		m_beginningTags.Add(CommandId.FileRecentlyUsed3);
		m_beginningTags.Add(CommandId.FileRecentlyUsed4);
		m_endingTags.Add(StandardCommand.FileExit);
		m_endingTags.Add(CommandId.EditPreferences);
		m_endingTags.Add(CommandId.EditDocumentPreferences);
		m_beginningTags.Add(StandardMenu.File);
		m_beginningTags.Add(StandardMenu.Edit);
		m_beginningTags.Add(StandardMenu.Format);
		m_beginningTags.Add(StandardMenu.View);
		m_beginningTags.Add(StandardMenu.Modify);
		m_endingTags.Add(StandardMenu.Help);
		m_endingTags.Add(StandardMenu.Window);
		m_defaultSortByMenuLabel.Add(StandardCommandGroup.WindowDocuments);
	}

	public int Compare(ICommandItem x, ICommandItem y)
	{
		return CompareCommands(x, y);
	}

	public int Compare(IMenuItem x, IMenuItem y)
	{
		if (x == null || y == null)
		{
			return 0;
		}
		ICommandItem commandItem = x as ICommandItem;
		ICommandItem commandItem2 = y as ICommandItem;
		if (commandItem != null && commandItem2 != null)
		{
			return CompareCommands(commandItem, commandItem2);
		}
		if (commandItem != null)
		{
			return 1;
		}
		if (commandItem2 != null)
		{
			return -1;
		}
		return 0;
	}

	public static bool TagsEqual(object tag1, object tag2)
	{
		return tag1?.Equals(tag2) ?? (tag2 == null);
	}

	public static int CompareCommands(ICommandItem x, ICommandItem y)
	{
		int num = CompareTags(x.MenuTag, y.MenuTag);
		if (num == 0)
		{
			num = CompareTags(x.GroupTag, y.GroupTag);
		}
		if (num == 0)
		{
			num = CompareTags(x.CommandTag, y.CommandTag);
		}
		if (num == 0)
		{
			num = ((x.GroupTag == null || !m_defaultSortByMenuLabel.Contains(x.GroupTag)) ? (x.Index - y.Index) : CompareTags(x.Text, y.Text));
		}
		return num;
	}

	public static int CompareTags(object tag1, object tag2)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (tag1 != null)
		{
			flag = m_beginningTags.Contains(tag1);
			flag3 = m_endingTags.Contains(tag1);
		}
		if (tag2 != null)
		{
			flag2 = m_beginningTags.Contains(tag2);
			flag4 = m_endingTags.Contains(tag2);
		}
		if (flag && !flag2)
		{
			return -1;
		}
		if (flag2 && !flag)
		{
			return 1;
		}
		if (flag3 && !flag4)
		{
			return 1;
		}
		if (flag4 && !flag3)
		{
			return -1;
		}
		if (tag1 is Enum && tag2 is Enum)
		{
			return ((int)tag1).CompareTo((int)tag2);
		}
		if (tag1 is Enum)
		{
			return -1;
		}
		if (tag2 is Enum)
		{
			return 1;
		}
		if (tag1 is string && tag2 is string)
		{
			return StringUtil.CompareNaturalOrder((string)tag1, (string)tag2);
		}
		IComparable comparable = tag1 as IComparable;
		IComparable comparable2 = tag2 as IComparable;
		if (comparable != null)
		{
			int num = comparable.CompareTo(tag2);
			if (num != 0)
			{
				return num;
			}
		}
		if (comparable2 != null)
		{
			int num2 = comparable2.CompareTo(tag1);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return 0;
	}
}
