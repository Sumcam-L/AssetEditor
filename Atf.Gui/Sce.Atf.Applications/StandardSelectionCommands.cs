using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(StandardSelectionCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardSelectionCommands : ICommandClient, IInitializable
{
	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[ImportingConstructor]
	public StandardSelectionCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
		m_contextRegistry.ActiveContextChanged += ActiveContextChanging;
		m_contextRegistry.ActiveContextChanged += ActiveContextChanged;
	}

	void IInitializable.Initialize()
	{
		CommandInfo.EditSelectAll.EnableCheckCanDoEvent(this);
		m_commandService.RegisterCommand(CommandInfo.EditSelectAll, this);
		CommandInfo.EditDeselectAll.EnableCheckCanDoEvent(this);
		m_commandService.RegisterCommand(CommandInfo.EditDeselectAll, this);
		CommandInfo.EditInvertSelection.EnableCheckCanDoEvent(this);
		m_commandService.RegisterCommand(CommandInfo.EditInvertSelection, this);
	}

	public bool SelectAll()
	{
		object activeContext = m_contextRegistry.ActiveContext;
		return SelectAll(activeContext.As<ISelectionContext>(), activeContext.As<IEnumerableContext>());
	}

	public bool SelectAll(ISelectionContext selectionContext, IEnumerableContext enumerableContext)
	{
		if (selectionContext != null && enumerableContext != null)
		{
			selectionContext.SetRange(enumerableContext.Items);
			return true;
		}
		return false;
	}

	public bool DeselectAll()
	{
		return DeselectAll(m_contextRegistry.GetActiveContext<ISelectionContext>());
	}

	public bool DeselectAll(ISelectionContext selectionContext)
	{
		if (selectionContext != null)
		{
			selectionContext.Clear();
			return true;
		}
		return false;
	}

	public bool InvertSelection()
	{
		return InvertSelection(m_contextRegistry.GetActiveContext<ISelectionContext>(), m_contextRegistry.GetActiveContext<IEnumerableContext>());
	}

	public bool InvertSelection(ISelectionContext selectionContext, IEnumerableContext enumerableContext)
	{
		if (selectionContext != null && enumerableContext != null)
		{
			HashSet<object> hashSet = new HashSet<object>(selectionContext.Selection);
			List<object> list = new List<object>(enumerableContext.Items);
			int num = 0;
			while (num < list.Count)
			{
				if (hashSet.Contains(list[num]))
				{
					list.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			selectionContext.SetRange(list);
		}
		return false;
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand)
		{
			ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
			IEnumerableContext activeContext2 = m_contextRegistry.GetActiveContext<IEnumerableContext>();
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.EditDeselectAll:
				result = activeContext != null && activeContext.LastSelected != null;
				break;
			case StandardCommand.EditSelectAll:
			case StandardCommand.EditInvertSelection:
				result = activeContext != null && activeContext2 != null;
				break;
			}
		}
		return result;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.EditSelectAll:
				SelectAll();
				break;
			case StandardCommand.EditDeselectAll:
				DeselectAll();
				break;
			case StandardCommand.EditInvertSelection:
				InvertSelection();
				break;
			}
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void ActiveContextChanging(object sender, EventArgs eventArgs)
	{
		ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		if (activeContext != null)
		{
			activeContext.SelectionChanged -= SelectionChanged;
		}
	}

	private void ActiveContextChanged(object sender, EventArgs eventArgs)
	{
		ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		if (activeContext != null)
		{
			activeContext.SelectionChanged += SelectionChanged;
		}
		CommandInfo.EditSelectAll.OnCheckCanDo(this);
		CommandInfo.EditDeselectAll.OnCheckCanDo(this);
		CommandInfo.EditInvertSelection.OnCheckCanDo(this);
	}

	private void SelectionChanged(object sender, EventArgs eventArgs)
	{
		CommandInfo.EditDeselectAll.OnCheckCanDo(this);
	}
}
