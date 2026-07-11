using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(StandardLayoutCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardLayoutCommands : ICommandClient, IInitializable
{
	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[ImportingConstructor]
	public StandardLayoutCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	public virtual void Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.FormatAlignLefts, this);
		m_commandService.RegisterCommand(CommandInfo.FormatAlignTops, this);
		m_commandService.RegisterCommand(CommandInfo.FormatAlignRights, this);
		m_commandService.RegisterCommand(CommandInfo.FormatAlignCenters, this);
		m_commandService.RegisterCommand(CommandInfo.FormatAlignBottoms, this);
		m_commandService.RegisterCommand(CommandInfo.FormatAlignMiddles, this);
		m_commandService.RegisterCommand(CommandInfo.FormatMakeWidthEqual, this);
		m_commandService.RegisterCommand(CommandInfo.FormatMakeHeightEqual, this);
		m_commandService.RegisterCommand(CommandInfo.FormatMakeSizeEqual, this);
	}

	public virtual void AlignLefts(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		LayoutContexts.GetBounds(layoutContext, items, out var bounds);
		foreach (object item in items)
		{
			layoutContext.SetBounds(item, bounds, BoundsSpecified.X);
		}
	}

	public virtual void AlignTops(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		LayoutContexts.GetBounds(layoutContext, items, out var bounds);
		foreach (object item in items)
		{
			layoutContext.SetBounds(item, bounds, BoundsSpecified.Y);
		}
	}

	public virtual void AlignRights(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		LayoutContexts.GetBounds(layoutContext, items, out var bounds);
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds2);
			bounds2.X = bounds.Right - bounds2.Width;
			layoutContext.SetBounds(item, bounds2, BoundsSpecified.X);
		}
	}

	public virtual void AlignBottoms(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		LayoutContexts.GetBounds(layoutContext, items, out var bounds);
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds2);
			bounds2.Y = bounds.Bottom - bounds2.Height;
			layoutContext.SetBounds(item, bounds2, BoundsSpecified.Y);
		}
	}

	public virtual void AlignCenters(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		LayoutContexts.GetBounds(layoutContext, items, out var bounds);
		int num = (bounds.Left + bounds.Right) / 2;
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds2);
			bounds2.X = num - bounds2.Width / 2;
			layoutContext.SetBounds(item, bounds2, BoundsSpecified.X);
		}
	}

	public virtual void AlignMiddles(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		LayoutContexts.GetBounds(layoutContext, items, out var bounds);
		int num = (bounds.Top + bounds.Bottom) / 2;
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds2);
			bounds2.Y = num - bounds2.Height / 2;
			layoutContext.SetBounds(item, bounds2, BoundsSpecified.Y);
		}
	}

	public virtual void MakeSizeEqual(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		Size maxSize = GetMaxSize(items, layoutContext);
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds);
			bounds.Size = maxSize;
			layoutContext.SetBounds(item, bounds, BoundsSpecified.Size);
		}
	}

	public virtual void MakeWidthEqual(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		Size maxSize = GetMaxSize(items, layoutContext);
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds);
			bounds.Width = maxSize.Width;
			layoutContext.SetBounds(item, bounds, BoundsSpecified.Width);
		}
	}

	public virtual void MakeHeightEqual(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		Size maxSize = GetMaxSize(items, layoutContext);
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds);
			bounds.Height = maxSize.Height;
			layoutContext.SetBounds(item, bounds, BoundsSpecified.Height);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		bool result = false;
		ILayoutContext activeContext = m_contextRegistry.GetActiveContext<ILayoutContext>();
		ISelectionContext activeContext2 = m_contextRegistry.GetActiveContext<ISelectionContext>();
		if (activeContext != null && activeContext2 != null)
		{
			BoundsSpecified boundsSpecified = GetBoundsSpecified(activeContext, activeContext2.Selection);
			if (boundsSpecified != BoundsSpecified.None)
			{
				switch ((StandardCommand)commandTag)
				{
				case StandardCommand.FormatAlignLefts:
				case StandardCommand.FormatAlignRights:
				case StandardCommand.FormatAlignCenters:
					result = (boundsSpecified & BoundsSpecified.X) != 0;
					break;
				case StandardCommand.FormatAlignTops:
				case StandardCommand.FormatAlignBottoms:
				case StandardCommand.FormatAlignMiddles:
					result = (boundsSpecified & BoundsSpecified.Y) != 0;
					break;
				case StandardCommand.FormatMakeWidthEqual:
					result = (boundsSpecified & BoundsSpecified.Width) != 0;
					break;
				case StandardCommand.FormatMakeHeightEqual:
					result = (boundsSpecified & BoundsSpecified.Height) != 0;
					break;
				case StandardCommand.FormatMakeSizeEqual:
					result = (boundsSpecified & BoundsSpecified.Size) == BoundsSpecified.Size;
					break;
				}
			}
		}
		return result;
	}

	public virtual void DoCommand(object commandTag)
	{
		ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		ILayoutContext layoutContext = m_contextRegistry.GetActiveContext<ILayoutContext>();
		if (layoutContext == null || activeContext == null)
		{
			return;
		}
		IEnumerable<object> items = activeContext.Selection;
		if (GetBoundsSpecified(layoutContext, activeContext.Selection) == BoundsSpecified.None)
		{
			return;
		}
		string transactionName = null;
		switch ((StandardCommand)commandTag)
		{
		case StandardCommand.FormatAlignLefts:
			transactionName = CommandInfo.FormatAlignLefts.MenuText;
			break;
		case StandardCommand.FormatAlignRights:
			transactionName = CommandInfo.FormatAlignRights.MenuText;
			break;
		case StandardCommand.FormatAlignCenters:
			transactionName = CommandInfo.FormatAlignCenters.MenuText;
			break;
		case StandardCommand.FormatAlignTops:
			transactionName = CommandInfo.FormatAlignTops.MenuText;
			break;
		case StandardCommand.FormatAlignBottoms:
			transactionName = CommandInfo.FormatAlignBottoms.MenuText;
			break;
		case StandardCommand.FormatAlignMiddles:
			transactionName = CommandInfo.FormatAlignMiddles.MenuText;
			break;
		case StandardCommand.FormatMakeWidthEqual:
			transactionName = CommandInfo.FormatMakeWidthEqual.MenuText;
			break;
		case StandardCommand.FormatMakeHeightEqual:
			transactionName = CommandInfo.FormatMakeHeightEqual.MenuText;
			break;
		case StandardCommand.FormatMakeSizeEqual:
			transactionName = CommandInfo.FormatMakeSizeEqual.MenuText;
			break;
		}
		ITransactionContext activeContext2 = m_contextRegistry.GetActiveContext<ITransactionContext>();
		activeContext2.DoTransaction(delegate
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.FormatAlignLefts:
				AlignLefts(items, layoutContext);
				break;
			case StandardCommand.FormatAlignRights:
				AlignRights(items, layoutContext);
				break;
			case StandardCommand.FormatAlignCenters:
				AlignCenters(items, layoutContext);
				break;
			case StandardCommand.FormatAlignTops:
				AlignTops(items, layoutContext);
				break;
			case StandardCommand.FormatAlignBottoms:
				AlignBottoms(items, layoutContext);
				break;
			case StandardCommand.FormatAlignMiddles:
				AlignMiddles(items, layoutContext);
				break;
			case StandardCommand.FormatMakeWidthEqual:
				MakeWidthEqual(items, layoutContext);
				break;
			case StandardCommand.FormatMakeHeightEqual:
				MakeHeightEqual(items, layoutContext);
				break;
			case StandardCommand.FormatMakeSizeEqual:
				MakeSizeEqual(items, layoutContext);
				break;
			case StandardCommand.FormatAlignToGrid:
				break;
			}
		}, transactionName);
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private BoundsSpecified GetBoundsSpecified(ILayoutContext layoutContext, IEnumerable<object> items)
	{
		BoundsSpecified boundsSpecified = BoundsSpecified.None;
		foreach (object item in items)
		{
			boundsSpecified |= layoutContext.CanSetBounds(item);
		}
		return boundsSpecified;
	}

	private Size GetMaxSize(IEnumerable<object> items, ILayoutContext layoutContext)
	{
		Size result = default(Size);
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds);
			result = new Size(Math.Max(result.Width, bounds.Width), Math.Max(result.Height, bounds.Height));
		}
		return result;
	}
}
