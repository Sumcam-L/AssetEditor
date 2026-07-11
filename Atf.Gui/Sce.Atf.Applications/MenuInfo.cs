using System;

namespace Sce.Atf.Applications;

public class MenuInfo
{
	public enum MenuAlignment
	{
		Left,
		Right
	}

	public readonly object MenuTag;

	public readonly string MenuText;

	public readonly string Description;

	public int Commands;

	public MenuAlignment Alignment = MenuAlignment.Left;

	private ICommandService m_commandService;

	public ICommandService CommandService
	{
		get
		{
			return m_commandService;
		}
		set
		{
			if (m_commandService != null)
			{
				throw new InvalidOperationException($"MenuInfo {MenuTag} already has been registered! MenuText=\"{MenuText}\"");
			}
			m_commandService = value;
		}
	}

	public MenuInfo(object menuTag, string menuText, string description)
	{
		MenuTag = menuTag;
		MenuText = menuText;
		Description = description;
	}
}
