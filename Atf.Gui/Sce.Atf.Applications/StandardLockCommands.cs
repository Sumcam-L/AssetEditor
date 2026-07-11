using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(StandardLockCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardLockCommands : ICommandClient, IInitializable
{
	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[ImportingConstructor]
	public StandardLockCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.EditLock, this);
		m_commandService.RegisterCommand(CommandInfo.EditUnlock, this);
	}

	public void Lock(IEnumerable<object> items, ILockingContext lockingContext)
	{
		foreach (object item in items)
		{
			if (lockingContext.CanSetLocked(item) && !lockingContext.IsLocked(item))
			{
				lockingContext.SetLocked(item, value: true);
			}
		}
	}

	public void Unlock(IEnumerable<object> items, ILockingContext lockingContext)
	{
		foreach (object item in items)
		{
			if (lockingContext.CanSetLocked(item) && lockingContext.IsLocked(item))
			{
				lockingContext.SetLocked(item, value: false);
			}
		}
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand)
		{
			ISelectionContext activeContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
			ILockingContext activeContext2 = m_contextRegistry.GetActiveContext<ILockingContext>();
			if (activeContext != null && activeContext2 != null)
			{
				switch ((StandardCommand)commandTag)
				{
				case StandardCommand.EditLock:
					foreach (object item in activeContext.Selection)
					{
						if (activeContext2.CanSetLocked(item) && !activeContext2.IsLocked(item))
						{
							result = true;
							break;
						}
					}
					break;
				case StandardCommand.EditUnlock:
					foreach (object item2 in activeContext.Selection)
					{
						if (activeContext2.CanSetLocked(item2) && activeContext2.IsLocked(item2))
						{
							result = true;
							break;
						}
					}
					break;
				}
			}
		}
		return result;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		ITransactionContext activeContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
		ILockingContext lockingContext = m_contextRegistry.GetActiveContext<ILockingContext>();
		if (!(commandTag is StandardCommand standardCommand))
		{
			return;
		}
		switch (standardCommand)
		{
		case StandardCommand.EditLock:
			activeContext.DoTransaction(delegate
			{
				Lock(selectionContext.Selection, lockingContext);
			}, CommandInfo.EditLock.MenuText);
			break;
		case StandardCommand.EditUnlock:
			activeContext.DoTransaction(delegate
			{
				Unlock(selectionContext.Selection, lockingContext);
			}, CommandInfo.EditUnlock.MenuText);
			break;
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}
}
