using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

public static class ApplicationUtil
{
	public static bool Insert(object context, object parent, object child, string operationName, IStatusService statusService)
	{
		ITransactionContext transactionContext = context.As<ITransactionContext>();
		if (CanInsert(context, parent, child))
		{
			if (transactionContext != null)
			{
				transactionContext.DoTransaction(delegate
				{
					DoInsert(context, parent, child);
				}, operationName);
			}
			else
			{
				DoInsert(context, parent, child);
			}
			statusService?.ShowStatus(operationName);
			return true;
		}
		return false;
	}

	public static bool CanInsert(object context, object parent, object child)
	{
		return context.As<IHierarchicalInsertionContext>()?.CanInsert(parent, child) ?? context.As<IInstancingContext>()?.CanInsert(child) ?? false;
	}

	private static void DoInsert(object context, object parent, object child)
	{
		IHierarchicalInsertionContext hierarchicalInsertionContext = context.As<IHierarchicalInsertionContext>();
		if (hierarchicalInsertionContext != null)
		{
			hierarchicalInsertionContext.Insert(parent, child);
		}
		else
		{
			context.As<IInstancingContext>()?.Insert(child);
		}
	}
}
