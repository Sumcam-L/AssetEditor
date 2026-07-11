using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(StandardViewCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardViewCommands : ICommandClient, IInitializable
{
	private ScriptingService m_scriptingService;

	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[ImportingConstructor]
	public StandardViewCommands(ICommandService commandService, IContextRegistry contextRegistry, ScriptingService scriptingService)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
		m_scriptingService = scriptingService;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.ViewFrameSelection, this);
		m_commandService.RegisterCommand(CommandInfo.ViewFrameAll, this);
		if (m_scriptingService != null)
		{
			m_scriptingService.SetVariable("atfView", this);
		}
	}

	public void FrameSelection()
	{
		object activeContext = m_contextRegistry.ActiveContext;
		ISelectionContext selectionContext = activeContext.As<ISelectionContext>();
		if (selectionContext != null)
		{
			FrameSelection(activeContext.As<IViewingContext>(), selectionContext.Selection);
		}
	}

	public void FrameSelection(IViewingContext viewingContext, IEnumerable<object> items)
	{
		if (viewingContext != null && viewingContext.CanFrame(items))
		{
			viewingContext.Frame(items);
		}
	}

	public void FrameAll()
	{
		object activeContext = m_contextRegistry.ActiveContext;
		IEnumerable<object> items = activeContext.As<IEnumerableContext>()?.Items;
		FrameAll(activeContext.As<IViewingContext>(), items);
	}

	public void FrameAll(IViewingContext viewingContext, IEnumerable<object> items)
	{
		if (items != null && viewingContext != null && viewingContext.CanFrame(items))
		{
			viewingContext.Frame(items);
		}
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand)
		{
			IViewingContext activeContext = m_contextRegistry.GetActiveContext<IViewingContext>();
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.ViewFrameSelection:
			{
				ISelectionContext activeContext3 = m_contextRegistry.GetActiveContext<ISelectionContext>();
				if (activeContext != null && activeContext3 != null)
				{
					result = activeContext.CanFrame(activeContext3.Selection);
				}
				break;
			}
			case StandardCommand.ViewFrameAll:
			{
				IEnumerableContext activeContext2 = m_contextRegistry.GetActiveContext<IEnumerableContext>();
				if (activeContext != null && activeContext2 != null)
				{
					result = activeContext.CanFrame(activeContext2.Items);
				}
				break;
			}
			}
		}
		return result;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand standardCommand)
		{
			switch (standardCommand)
			{
			case StandardCommand.ViewFrameSelection:
				FrameSelection();
				break;
			case StandardCommand.ViewFrameAll:
				FrameAll();
				break;
			}
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}
}
