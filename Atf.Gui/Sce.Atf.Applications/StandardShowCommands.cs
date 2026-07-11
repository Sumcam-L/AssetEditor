using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[Export(typeof(StandardShowCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardShowCommands : IInitializable, ICommandClient, IContextMenuCommandProvider
{
	private readonly Stack<IList<WeakReference>> m_hideStack = new Stack<IList<WeakReference>>();

	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[ImportingConstructor]
	public StandardShowCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.ViewHide, this);
		m_commandService.RegisterCommand(CommandInfo.ViewShow, this);
		m_commandService.RegisterCommand(CommandInfo.ViewShowLast, this);
		m_commandService.RegisterCommand(CommandInfo.ViewShowAll, this);
		m_commandService.RegisterCommand(CommandInfo.ViewIsolate, this);
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
			IEnumerableContext activeContext2 = m_contextRegistry.GetActiveContext<IEnumerableContext>();
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.ViewHide:
				return activeContext != null && activeContext.SelectionCount > 0;
			case StandardCommand.ViewShow:
				return activeContext != null && activeContext.SelectionCount > 0;
			case StandardCommand.ViewShowLast:
				return m_hideStack != null && m_hideStack.Count > 0;
			case StandardCommand.ViewShowAll:
				return activeContext2 != null;
			case StandardCommand.ViewIsolate:
				return activeContext != null && activeContext.SelectionCount > 0;
			}
		}
		return false;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
			IEnumerableContext activeContext2 = m_contextRegistry.GetActiveContext<IEnumerableContext>();
			IVisibilityContext activeContext3 = m_contextRegistry.GetActiveContext<IVisibilityContext>();
			IEnumerable<object> selection = null;
			if (activeContext != null)
			{
				selection = activeContext.Selection;
			}
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.ViewHide:
				HideSelection(selection, activeContext3);
				break;
			case StandardCommand.ViewShow:
				ShowSelection(selection, activeContext3);
				break;
			case StandardCommand.ViewShowLast:
				ShowLastHidden(activeContext3);
				break;
			case StandardCommand.ViewShowAll:
				ShowAll(activeContext3, activeContext2);
				break;
			case StandardCommand.ViewIsolate:
				IsolateSelection(selection, activeContext3, activeContext2);
				break;
			}
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void HideSelection(IEnumerable<object> selection, IVisibilityContext visibilityContext)
	{
		if (selection == null || visibilityContext == null)
		{
			return;
		}
		List<WeakReference> list = new List<WeakReference>();
		foreach (object item in selection)
		{
			list.Add(new WeakReference(item));
		}
		foreach (object item2 in selection)
		{
			visibilityContext.SetVisible(item2, value: false);
		}
		m_hideStack.Push(list);
		Refresh();
	}

	private void ShowSelection(IEnumerable<object> selection, IVisibilityContext visibilityContext)
	{
		if (selection == null || visibilityContext == null)
		{
			return;
		}
		foreach (object item in selection)
		{
			visibilityContext.SetVisible(item, value: true);
		}
		Refresh();
	}

	public void ShowAll(IVisibilityContext visibilityContext, IEnumerableContext enumerableContext)
	{
		if (visibilityContext == null || enumerableContext == null)
		{
			return;
		}
		foreach (object item in enumerableContext.Items)
		{
			visibilityContext.SetVisible(item, value: true);
		}
		Refresh();
	}

	private void ShowLastHidden(IVisibilityContext visibilityContext)
	{
		if (m_hideStack.Count <= 0)
		{
			return;
		}
		IList<WeakReference> list = m_hideStack.Pop();
		foreach (WeakReference item in list)
		{
			object target = item.Target;
			if (target != null)
			{
				visibilityContext.SetVisible(target, value: true);
			}
		}
		Refresh();
	}

	private void IsolateSelection(IEnumerable<object> selection, IVisibilityContext visibilityContext, IEnumerableContext enumerableContext)
	{
		if (selection == null || visibilityContext == null || enumerableContext == null)
		{
			return;
		}
		foreach (object item in enumerableContext.Items)
		{
			visibilityContext.SetVisible(item, value: false);
		}
		foreach (object item2 in selection)
		{
			visibilityContext.SetVisible(item2, value: true);
		}
		Refresh();
	}

	protected virtual void Refresh()
	{
	}

	public virtual IEnumerable<object> GetCommands(object context, object target)
	{
		yield return StandardCommand.ViewHide;
		yield return StandardCommand.ViewShow;
		yield return StandardCommand.ViewShowLast;
		yield return StandardCommand.ViewShowAll;
		yield return StandardCommand.ViewIsolate;
	}
}
